using System;
using System.Net;
using SimpleRUDP.Channels;
using SimpleRUDP.Channels.System;
using SimpleRUDP.Peers.Remote;
using SimpleRUDP.Protocol;

namespace SimpleRUDP.Peers.Server
{
    public partial class Server
    {
        private void ProcessHandshake(IPEndPoint endPoint, byte[] datagram)
        {
            if (_connectedPeers.ContainsKey(endPoint.GetHashCode())) return;
            
            RemotePeer remotePeer = new RemotePeer(endPoint, this);
            _connectedPeers.Add(endPoint.GetHashCode(), remotePeer);

            byte[] handshakeResponse = new byte[4];
            handshakeResponse[0] = (byte) PacketId.ServerHandshakeAction;
            handshakeResponse[1] = datagram[1];
            handshakeResponse[2] = datagram[2];
            
            if (_connectedPeers.Count + 1 > _maxPeers)
                handshakeResponse[3] = 0; // false
            
            else
                handshakeResponse[3] = 1; // true;

            SystemChannel channel = (SystemChannel) remotePeer.Channels[(byte) ChannelId.ReliableUnorderedUnefficient];
            channel.SendPacket(handshakeResponse, 4, PacketId.ServerHandshakeAction, endPoint);
        }
    }
}