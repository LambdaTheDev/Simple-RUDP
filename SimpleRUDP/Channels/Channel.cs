using System.Net;
using SimpleRUDP.Channels.System;
using SimpleRUDP.Channels.Unreliable;
using SimpleRUDP.Peers;

namespace SimpleRUDP.Channels
{
    public abstract class Channel
    {
        public const int Mtu = 1500; // Maximum packet size, 1500 bytes is a real-world max UDP packet size

        protected readonly ChannelBuffer Buffer = new ChannelBuffer(Mtu); // Buffer used to store data to send
        protected readonly IRawPeer BoundPeer; // Peer associated with this Channel

        public abstract int ChannelHeaderSize { get; }

        protected Channel(IRawPeer boundPeer)
        {
            BoundPeer = boundPeer;
        }
        
        // Sends data buffer to target
        public abstract void Send(byte[] data, int length, IPEndPoint target);
        
        // Handles incoming packet from peer.
        public abstract void Handle(byte[] data, int offset, IPEndPoint target);
        
        
        // Used by peers, returns channel list
        internal static Channel[] SetupChannels(IRawPeer peer)
        {
            Channel[] channels = new Channel[5]; // Channels count
            
            channels[(byte) ChannelId.ReliableUnorderedUnefficient] = new SystemChannel(peer);
            channels[(byte) ChannelId.Unreliable] = new UnreliableChannel(peer);

            return channels;
        }
    }
}