using System;
using System.Net;

namespace SimpleRUDP.Peers.Remote
{
    public partial class RemotePeer
    {
        public void HandleDataPacket(byte[] buffer, int offset, int length, IPEndPoint sender)
        {
            _server.HandleDataPacket(buffer, offset, length, sender);
        }

        public void HandleInternalPacket(byte[] buffer, int offset, int length, IPEndPoint sender)
        {
            _server.HandleInternalPacket(buffer, offset, length, sender);
        }
    }
}