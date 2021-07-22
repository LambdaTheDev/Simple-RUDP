using System;
using System.Net;

namespace SimpleRUDP.Peers.Server
{
    public partial class Server
    {
        public override void HandleDataPacket(byte[] buffer, int offset, int length, IPEndPoint sender)
        {
            // Simple workaround - client ID is hash code of sender.
            // But in production, IDs (imo) should be random
            
            int peerId = sender.GetHashCode();
            DataReceivedFromClientEvent?.Invoke(peerId, buffer, offset, length);
        }

        public override void HandleInternalPacket(byte[] buffer, int offset, int length, IPEndPoint sender)
        {
        }
    }
}