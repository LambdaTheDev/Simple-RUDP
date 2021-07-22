using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using SimpleRUDP.Channels;
using SimpleRUDP.Protocol;
using SimpleRUDP.States;

namespace SimpleRUDP.Peers
{
    // Server or Client peer, not Remote one
    public abstract class LocalPeer : IRawPeer
    {
        protected UdpClient Udp; // UdpClient instance bound with this peer

        public Channel[] Channels { get; }
        public PeerState State { get; protected set; } = PeerState.Disconnected; // Current state of this peer
        public IPEndPoint TargetEndPoint { get; protected set; } // Associated IPEndPoint
        public bool IsReady => State == PeerState.Connected && _isListening; // Returns true, if connected & is listening


        // Property used to start/stop listening
        private bool _isListening;
        public bool IsListening
        {
            get => _isListening;
            set
            {
                _isListening = value;
                if(value) StartListening();
                else StopListening();
            }
        }

        public event Action PeerStartedEvent; // Called when peer successfully starts
        public event Action PeerStoppingEvent; // Called when peer begins stopping (on server, before disconnecting clients)
        public event Action PeerStoppedEvent; // Called when peer is successfully stopped


        public LocalPeer()
        {
            Channels = Channel.SetupChannels(this);
        }
        
        
        // No need for abstract, it just sends UDP datagram. High level methods are
        // used for channels and stuff
        public void SendRawDatagram(byte[] datagram, int length, IPEndPoint target)
        {
            Udp.Send(datagram, length, target);
        }

        public abstract void HandleDataPacket(byte[] buffer, int offset, int length, IPEndPoint sender);
        public abstract void HandleInternalPacket(byte[] buffer, int offset, int length, IPEndPoint sender);

        // Actions when datagram has been received
        public abstract void OnRawDataReceived(byte[] datagram, IPEndPoint sender);

        // Method used to start listening for datagrams
        private void StartListening()
        {
            __InternalListen();
        }

        // Task that awaits for UDP datagrams
        private async Task __InternalListen()
        {
            while (_isListening)
            {
                UdpReceiveResult incomingDatagram = await Udp.ReceiveAsync();
                OnRawDataReceived(incomingDatagram.Buffer, incomingDatagram.RemoteEndPoint);
            }
        }
        
        // Sends empty packet to itself to break __InternalListen loop
        private void StopListening()
        {
            byte[] emptyPacket = { (byte) PacketId.Empty };
            Udp.Send(emptyPacket, 1, TargetEndPoint);
        }

        public abstract void InvokeDataReceived(byte[] data, int offset, IRawPeer from);

        #region Event invokers

        // C# Doesn't allow invoking events from child classes, so I did
        // those helper protected methods

        protected void InvokePeerStarted()
        {
            PeerStartedEvent?.Invoke();
        }

        protected void InvokePeerStopping()
        {
            PeerStoppingEvent?.Invoke();
        }

        protected void InvokePeerStopped()
        {
            PeerStoppedEvent?.Invoke();
        }
        
        #endregion
    }
}