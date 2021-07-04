using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Simple_RUDP
{
    public class Server : Common
    {
        private readonly Dictionary<int, ClientConnection> _connectedClients = new Dictionary<int, ClientConnection>();
        private readonly int _maxConnections; // Max connections

        private bool _isClosing; // Returns true, if server is being closed.
        
        public Server(int maxConnections)
        {
            _maxConnections = maxConnections;
        }
        
        // Starts the server
        public override void Start(IPAddress targetIp, ushort port)
        {
            Udp = new UdpClient(new IPEndPoint(targetIp, port));
            ListenerTask = Listen();
        }

        // Here server sends reliable disconnect packets to all peers & stops itself
        // This starts async task
        public override void Stop()
        {
            _isClosing = true;
        }

        protected override void HandleIncomingPacket(IPEndPoint endPoint, byte[] datagram)
        {
            throw new System.NotImplementedException();
        }
    }

    // This struct represents client connection to server
    internal struct ClientConnection
    {
        public int Id => EndPoint.GetHashCode();
        public IPEndPoint EndPoint;
        public ClientState State;
    }

    // Represents state of remote client
    internal enum ClientState : byte
    {
        Offline, // Client is offline
        Connecting, // Client sent connection request, but isn't accepted by server
        Connected, // Server accepted connection request
    }
}