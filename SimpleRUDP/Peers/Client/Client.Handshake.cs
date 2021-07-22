using System;
using System.Threading.Tasks;
using SimpleRUDP.Protocol;
using SimpleRUDP.States;

namespace SimpleRUDP.Peers.Client
{
    public partial class Client
    {
        public const int MaxConnectionAttempts = 5;
        public const int ConnectionPacketReceiveDelay = 300; // How much ms between connection packet sent & check if it was delivered
        
        private ConnectionAttemptData _connectionAttemptData;

        private async Task<ConnectionAttemptState> TryToConnect()
        {
            _connectionAttemptData.Start();
            
            byte[] requestDatagram = new byte[3];
            requestDatagram[0] = (byte) PacketId.ClientHandshakeAction;
            requestDatagram[1] = _connectionAttemptData.RequestId[0];
            requestDatagram[2] = _connectionAttemptData.RequestId[1];
            
            for (byte i = 0; i < MaxConnectionAttempts; i++)
            {
                await Udp.SendAsync(requestDatagram, 3);
                await Task.Delay(ConnectionPacketReceiveDelay);
                
                if(!_connectionAttemptData.IsHandshakeAcked) continue;

                if (!_connectionAttemptData.IsAccepted)
                    return ConnectionAttemptState.ServerRejected;
            }
            
            // If still not acked, then unreachable
            if (!_connectionAttemptData.IsHandshakeAcked)
                return ConnectionAttemptState.ServerUnreachable;
            
            // Finish connection attempt & return
            _connectionAttemptData.Stop();
            return ConnectionAttemptState.Connected;
        }

        private void OnHandshakeAcked(bool wasAccepted)
        {
            _connectionAttemptData.IsHandshakeAcked = true;
            _connectionAttemptData.IsAccepted = wasAccepted;
        }
        
        // Struct to store info about attempt data
        private struct ConnectionAttemptData
        {
            public bool TryingToConnect;
            public byte[] RequestId; // 2 bytes
            public bool IsHandshakeAcked;
            public bool IsAccepted;

            public void Start()
            {
                RequestId = new byte[2];
                Rng.GetBytes(RequestId);
                TryingToConnect = true;
                IsHandshakeAcked = false;
                IsAccepted = false;
            }

            public void Stop()
            {
                TryingToConnect = false;
            }
        }
    }
}