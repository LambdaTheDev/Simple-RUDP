using System;
using System.Net;
using SimpleRUDP.Protocol;

namespace SimpleRUDP.Peers.Client
{
    public partial class Client
    {
        public override void HandleDataPacket(byte[] buffer, int offset, int length, IPEndPoint sender)
        {
            Console.WriteLine("Data packet");
        }

        public override void HandleInternalPacket(byte[] buffer, int offset, int length, IPEndPoint sender)
        {
            if (buffer[offset - 2] == (byte) PacketId.ServerHandshakeAction)
            {
                if (buffer.Length == 7)
                {
                    bool isAccepted = buffer[6] == 1;

                    // If bytes does not match, then destroy
                    if (_connectionAttemptData.RequestId[0] != buffer[4] ||
                        _connectionAttemptData.RequestId[1] != buffer[5])
                        return;
                    
                    OnHandshakeAcked(isAccepted);
                    return;
                }
            }
        }
    }
}