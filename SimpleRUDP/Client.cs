using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using SimpleRUDP.Exceptions;
using SimpleRUDP.Protocol;
using SimpleRUDP.States;

namespace SimpleRUDP
{
    public class Client : Peer
    {
        public const int MaxConnectionAttempts = 5;
        public const int ConnectionPacketReceiveDelay = 200; // How much ms between connection packet sent & check if it was delivered
        
        private static readonly RNGCryptoServiceProvider _rng = new RNGCryptoServiceProvider();

        private ConnectionAttemptData _connectionData = new ConnectionAttemptData();
        private CancellationTokenSource _connectionRequestTokenSource = new CancellationTokenSource();
        
        public event Action ConnectedToServerEvent; // Called when client successfully connects to server
        public event Action DisconnectedFromServerEvent; // Called when client successfully disconnects from server
        public event Action<byte[], int, int> DataReceivedFromServerEvent; // Called when server sends data | args: Buffer, Offset, Length 
        
        public Client() : base(true) { }

        public async Task<ConnectionAttemptState> Connect(string ip, ushort port)
        {
            ThrowIfUsed();
            State = PeerState.Connecting;
            
            Udp = new UdpClient();
            Udp.Connect(ip, port);
            InvokePeerStarted();
            
            ConnectionAttemptState connectionResult = await TryToConnect();
            if(connectionResult == ConnectionAttemptState.Success)
            {
                // Set state to connected & start listening for "normal" packets
                State = PeerState.Connected;
                StartListening();
            
                // Fire Connected event & return success
                ConnectedToServerEvent?.Invoke();
                return ConnectionAttemptState.Success;
            }

            return connectionResult;
        }

        private async Task<ConnectionAttemptState> TryToConnect()
        {
            // Resets connection data
            _connectionData.Start();

            // Create datagram for connection request
            ListenForConnectionDatagrams();
            byte[] connectionRequestDatagram = new byte[3];
            connectionRequestDatagram[0] = (byte) PacketId.ClientHandshakeRequest;
            Buffer.BlockCopy(_connectionData.RequestId, 0, connectionRequestDatagram, 1, 2);

            // Start sending connection request packets & await for server responses
            _connectionData.HandshakeState = 1;
            for (byte i = 0; i < MaxConnectionAttempts; i++)
            {
                await Udp.SendAsync(connectionRequestDatagram, 3);
                await Task.Delay(ConnectionPacketReceiveDelay);
                
                if(!_connectionData.HandshakeAcked) continue;

                if (!_connectionData.HandshakeAccepted)
                    return ConnectionAttemptState.ServerRejected;
            }

            // If client left the loop & server didn't acked request,
            // then server is unreachable or there is too high packet loss
            if (!_connectionData.HandshakeAcked)
                return ConnectionAttemptState.ServerUnreachable;

            // Set state to server acked, terminate listening for client datagram tasks
            _connectionData.HandshakeState = 2;
            _connectionRequestTokenSource.Cancel();
            _connectionRequestTokenSource.Dispose();

            // Create datagram for client confirmation, it's sent once, reliability here prob
            // isn't so important as previously.
            byte[] clientConfirmationDatagram = { (byte) PacketId.ClientHandshakeConfirmation };
            await Udp.SendAsync(clientConfirmationDatagram, 1);
            
            return ConnectionAttemptState.Success;
        }

        private void ListenForConnectionDatagrams()
        {
            Task.Run(async () =>
            {
                UdpReceiveResult receivedPacket = await Udp.ReceiveAsync();
                
                // Packet ID + sizeof(short) + is accepted
                if (receivedPacket.Buffer.Length == 4)
                {
                    if (receivedPacket.Buffer[0] == (byte) PacketId.ServerHandshakeAck)
                    {
                        // Server acked this connection attempt with this ID
                        if (_connectionData.RequestId[0] == receivedPacket.Buffer[1] &&
                            _connectionData.RequestId[1] == receivedPacket.Buffer[2])
                        {
                            _connectionData.HandshakeAcked = true;
                            _connectionData.HandshakeAccepted = receivedPacket.Buffer[3] == 1; // If == 1, then server responded with true
                        }
                    }
                }
            }, _connectionRequestTokenSource.Token);
        }

        public void Disconnect()
        {
            InvokePeerStopping();
            
            // todo: send packet about disconnection
            
            InvokePeerStopped();
        }

        private void ThrowIfUsed()
        {
            if(State != PeerState.Offline)
                throw new PeerUsedException();
        }
        
        protected override void SendRawDatagram(IPEndPoint receiver, byte[] datagram, int offset, int length)
        {
            Udp.Send(datagram, length, receiver);
        }

        protected override void ReceiveRawDatagram(IPEndPoint sender, byte[] datagram)
        {
        }

        // Data used by connecting feature
        private struct ConnectionAttemptData
        {
            public byte HandshakeState; // 0 - Not started, 1 - client sends handshake, awaits for server,
                                        // 2 - server acked request, 3 - client sent confirmation

            public byte[] RequestId;
            public bool HandshakeAcked; // True, if handshake ack has been received
            public bool HandshakeAccepted; // True, if server accepted client

            public void Start()
            {
                Reset();
                Client._rng.GetBytes(RequestId);
            }
            
            public void Reset()
            {
                HandshakeState = 0;
                RequestId = new byte[2];
                HandshakeAcked = false;
                HandshakeAccepted = false;
            }
        }
    }
}