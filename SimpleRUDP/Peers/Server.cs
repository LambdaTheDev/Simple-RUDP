using System;
using System.Net;
using System.Net.Sockets;
using Simple_RUDP.Exceptions;
using Simple_RUDP.Protocol;
using Simple_RUDP.Utils;

namespace Simple_RUDP.Peers
{
    public class Server : Peer
    {
        // Buffer used for quick responses, like connection response
        private readonly byte[] _smallBuffer = new byte[8];
        
        private ThreadSafeDictionary<int, RemotePeer> _connectedPeers;
        private int _maxPeers;

        // Peer initialization
        public Server() : base(false) { }

        // Starts the server 
        public void Start(ushort port, int maxPeers)
        {
            ThrowIfUsed();
            
            State = PeerState.Connecting;

            _maxPeers = maxPeers;
            _connectedPeers = new ThreadSafeDictionary<int, RemotePeer>(maxPeers);
            
            Udp = new UdpClient(new IPEndPoint(IPAddress.Any, port));
            ListeningTask = ListenForDatagrams();
        }

        // Stops the server & disconnects remote peers
        public void Stop()
        {
            // If disconnected or already disconnecting, then why to continue?
            if (State == PeerState.Disconnected || State == PeerState.Disconnecting) return;

            State = PeerState.Disconnecting;
            Terminate();
        }
        
        protected override void HandleIncomingDatagram(IPEndPoint packetSender, byte[] datagram)
        {
            if (_connectedPeers.TryGetValue(packetSender.GetHashCode(), out RemotePeer peer))
            {
                // todo: Work on the rest of packets
            }
            else
            {
                if (datagram[0] == (byte) PacketIds.ClientWantsToConnect)
                {
                    if (_connectedPeers.Count + 1 > _maxPeers)
                    {
                        _smallBuffer[0] = (byte) PacketIds.ServerConnectionResponse;
                        _smallBuffer[1] = 0; // false value
                        SendDatagram(packetSender, _smallBuffer, 0, 2);
                        return;
                    }
                    
                    RemotePeer newPeer = new RemotePeer
                    {
                        EndPoint = packetSender,
                        State = PeerState.Connected
                    };
                    
                    _connectedPeers.Add(newPeer.Id, newPeer);

                    _smallBuffer[0] = (byte) PacketIds.ServerConnectionResponse;
                    _smallBuffer[1] = 1; // true value
                    SendDatagram(packetSender, _smallBuffer, 0, 2);
                    
                    Console.WriteLine("New client connected to the server!");
                }
                
                // ELSE - Disregard?
            }
        }

        public override void SendDatagram(IPEndPoint target, byte[] datagram, int offset, int length)
        {
            Udp.Send(datagram, length, target);
        }

        public override void Terminate()
        {
            IsStopping = true;
            State = PeerState.Disconnecting;
            
            AsyncTerminate();
        }

        private async void AsyncTerminate()
        {
            // Disconnect all connections
            foreach (RemotePeer peer in _connectedPeers.Values)
            {
                // todo disconnect
            }

            Udp.Close();
            Udp.Dispose();
            
            State = PeerState.Disconnected;
        }

        // Throws exception if peer is already used
        private void ThrowIfUsed()
        {
            if(State != PeerState.Disconnected)
                throw new PeerAlreadyUsedException();
        }
    }
}