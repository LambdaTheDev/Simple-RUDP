using System;
using System.Net;
using System.Net.Sockets;
using Simple_RUDP.Protocol;

namespace Simple_RUDP.Peers
{
    public class Client : Peer
    {
        private readonly byte[] _smallBuffer = new byte[8]; // Small buffer used to quick requests
        private IPEndPoint _server; // EndPoint of server
        
        // Peer initialization
        public Client() : base(true) { }

        
        // Connects to specific server
        public void Connect(string ip, ushort port)
        {
            State = PeerState.Connecting;
            
            IPAddress serverAddress = IPAddress.Parse(ip);
            _server = new IPEndPoint(serverAddress, port);
            
            Udp = new UdpClient(_server);
            ListeningTask = ListenForDatagrams();
        }

        // Disconnects from server & stops client
        public void Disconnect()
        {
            State = PeerState.Disconnecting;

            _smallBuffer[0] = (byte) PacketIds.ClientWantsToConnect;
            AsyncDisconnect();
        }

        private async void AsyncDisconnect()
        {
            // Rn no reliable channel is done, let's send 10 packets
            // & hope for the best rn :P
            for (int i = 0; i < 10; i++)
            {
                await Udp.SendAsync(_smallBuffer, 1);
            }
            
            Udp.Close();
            Udp.Dispose();

            State = PeerState.Disconnected;
        }
        
        protected override void HandleIncomingDatagram(IPEndPoint packetSender, byte[] datagram)
        {
            Console.WriteLine("Received from server: " + datagram);
        }

        public void SendDatagramToServer(byte[] datagram, int offset, int length)
        {
            SendDatagram(_server, datagram, offset, length);
        }
        
        public override void SendDatagram(IPEndPoint target, byte[] datagram, int offset, int length)
        {
            Udp.Send(datagram, length);
        }

        public override void Terminate()
        {
            State = PeerState.Disconnecting;
            Udp.Close();
            Udp.Dispose();
            State = PeerState.Disconnected;
        }
    }
}