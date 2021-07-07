using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Simple_RUDP.Channels;
using Simple_RUDP.Channels.Reliable;
using Simple_RUDP.Channels.SemiReliable;
using Simple_RUDP.Channels.Unreliable;
using Simple_RUDP.Exceptions;

namespace Simple_RUDP.Peers
{
    // Client and server are peers, and in most cases they do similar stuff.
    public abstract class Peer
    {
        protected readonly bool IsClient; // Returns true, if peer is a client (not a server).
        protected readonly IChannel[] Channels = new IChannel[3]; // Channel instances for this peer

        protected bool IsReady => State == PeerState.Connected && !IsStopping; // Returns true, if peer is connected

        protected UdpClient Udp; // A UdpClient instance for each peer.
        protected PeerState State = PeerState.Disconnected; // Current state of peer
        protected Task ListeningTask; // Reference to async listening Tasks
        protected bool IsStopping; // Returns true, if this peer wants to be stopped (is disconnecting)


        protected Peer(bool isClient)
        {
            IsClient = isClient;
            
            // Channel setup
            Channels[(int) ChannelId.Reliable] = new ReliableChannel(this);
            Channels[(int) ChannelId.SemiReliable] = new SemiReliable(this);
            Channels[(int) ChannelId.Unreliable] = new UnreliableChannel(this);
        }

        // Method called by ListenForDatagrams, it lets Peer to handle incoming packets on their own.
        protected abstract void HandleIncomingDatagram(IPEndPoint packetSender, byte[] datagram);

        public abstract void SendDatagram(IPEndPoint target, byte[] datagram, int offset, int length);
        
        // Called by connect/start method in Peer when UdpClient is successfully initialized.
        protected async Task ListenForDatagrams()
        {
            if(Udp == null)
                throw new PeerNotReadyException("Peer must be enabled and connected before listening!");

            State = PeerState.Connected;
            
            while (IsReady)
            {
                UdpReceiveResult datagramInstance = await Udp.ReceiveAsync();
                HandleIncomingDatagram(datagramInstance.RemoteEndPoint, datagramInstance.Buffer);
            }
        }

        // Terminates this Peer. Note - remember to send packet to end Listening loop.
        public abstract void Terminate();
    }
}