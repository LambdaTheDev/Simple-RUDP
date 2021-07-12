using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SimpleRUDP.States;

namespace SimpleRUDP
{
    public abstract class Peer
    {
        protected readonly bool IsClient; // Returns true, if this is a client.

        public PeerState State { get; protected set; } = PeerState.Offline; // Current Peer state
        public bool IsReady => State == PeerState.Connected && !Stopping; // Returns true, if peer is ready

        protected UdpClient Udp; // Associated UdpClient
        protected Thread ReceivingThread; // Thread used to receive datagrams, kept to abort it on stop 
        protected bool Stopping; // This is true, when Peer is being stopped & is preparing for clean-up

        protected event Action PeerStartedListeningEvent; // Invoked when Peer starts listening
        protected event Action PeerStoppedListeningEvent; // Invoked when Peer stops listening
        

        // Constructor
        public Peer(bool isClient)
        {
            IsClient = isClient;
        }

        
        // Here Peers implement logic for sending raw datagrams
        protected abstract void SendRawDatagram(IPEndPoint receiver, byte[] datagram, int offset, int length);
        
        // Here Peers implement logic when they receive raw datagram
        protected abstract void ReceiveRawDatagram(IPEndPoint sender, byte[] datagram);

        
        // Peer calls this method to start listening for packets.
        // It creates a new Thread, so eventual collections need to be thread safe!
        protected void StartListening()
        {
            Thread listenThread = new Thread(() =>
            {
                __InternalListen();
            });
            
            listenThread.IsBackground = true;
            listenThread.Start();

            ReceivingThread = listenThread;
        }

        // Actual listening handler.
        private async Task __InternalListen()
        {
            try
            {
                PeerStartedListeningEvent?.Invoke();
                
                while (IsReady)
                {
                    UdpReceiveResult incomingDatagram = await Udp.ReceiveAsync();
                    ReceiveRawDatagram(incomingDatagram.RemoteEndPoint, incomingDatagram.Buffer);
                }
            }
            catch (ThreadAbortException) // Exception thrown when thread is stopped, just do clean-up
            {
                PeerStoppedListeningEvent?.Invoke();
            }
        }

        // Peer calls this method to stop listening. It aborts the thread.
        protected void StopListening()
        {
            Stopping = true;
            ReceivingThread.Abort();
        }
    }
}