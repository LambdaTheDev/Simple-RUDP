using System;
using System.Net;
using SimpleRUDP.Peers;
using SimpleRUDP.Protocol;

namespace SimpleRUDP.Channels.Unreliable
{
    public class UnreliableChannel : Channel
    {
        public UnreliableChannel(IRawPeer boundPeer) : base(boundPeer)
        { }

        public override int ChannelHeaderSize { get; } = 0;
        
        public override void Send(byte[] data, int length, IPEndPoint target)
        {
            Buffer.Buffer[0] = (byte) PacketId.ChannelPacket;
            Buffer.Buffer[1] = (byte) ChannelId.Unreliable;
            Buffer.Buffer[2] = (byte) PacketId.DataPacket;
            global::System.Buffer.BlockCopy(data, 0, Buffer.Buffer, 3, length);
            
            BoundPeer.SendRawDatagram(Buffer.Buffer, length + 3, target);
        }

        public override void Handle(byte[] data, int offset, IPEndPoint target)
        {
            BoundPeer.HandleDataPacket(data, offset, data.Length, target);
        }
    }
}