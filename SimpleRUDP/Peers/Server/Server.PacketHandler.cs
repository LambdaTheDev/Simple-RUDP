using System.Net;

namespace SimpleRUDP.Peers.Server
{
    public partial class Server
    {
        public override void HandleDataPacket(byte[] buffer, int offset, int length, IPEndPoint sender)
        {
        }

        public override void HandleInternalPacket(byte[] buffer, int offset, int length, IPEndPoint sender)
        {
        }
    }
}