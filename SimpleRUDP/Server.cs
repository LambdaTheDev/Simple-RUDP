using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using SimpleRUDP.Exceptions;
using SimpleRUDP.States;

namespace SimpleRUDP
{
    public sealed class Server : Peer
    {
        private Dictionary<int, RemotePeer> _connectedPeers;
        private int _maxClients;

        public event Action ServerStartedEvent; // Called when server successfully starts
        public event Action ServerStoppingEvent; // Called when server begins stopping (before disconnecting clients)
        public event Action ServerStoppedEvent; // Called when server successfully stops
        public event Action<int> PeerConnectedEvent; // Called when remote peer connects | args: PeerId
        public event Action<int> PeerDisconnectedEvent; // Called when remote peer disconnects | args: PeerId
        public event Action<int, byte[], int, int> PeerDataReceivedEvent; // Called when remote peer sends data | args: PeerId, Buffer, Offset, Length

        public Server() : base(false) { }
        
        
        // Method that initializes Server peer
        public void StartServer(ushort port, int maxClients)
        {
            ThrowIfCantStart();
            
            _connectedPeers = new Dictionary<int, RemotePeer>(maxClients);
            _maxClients = maxClients;
            Udp = new UdpClient(new IPEndPoint(IPAddress.Any, port));
            State = PeerState.Connected;

            StartListening();
            ServerStartedEvent?.Invoke();
        }

        // Method that stops server peer
        public void StopServer()
        {
            if (State == PeerState.Offline || State == PeerState.Disconnecting) return;
            State = PeerState.Disconnecting;
            ServerStoppingEvent?.Invoke();
            
            // todo: Send reliable Disconnection packet to all peers, async operation 
            
            ServerStoppedEvent?.Invoke();
        }

        private void ThrowIfCantStart()
        {
            if(State != PeerState.Offline)
                throw new PeerUsedException();
        }

        public void Send(int peerId, byte[] buffer, int offset, int length)
        {
            if (_connectedPeers.TryGetValue(peerId, out RemotePeer peer))
            {
                // todo: Pack packet to channels, create headers etc
                SendRawDatagram(peer.EndPoint, buffer, offset, length);
            }
        }
        
        
        protected override void SendRawDatagram(IPEndPoint receiver, byte[] datagram, int offset, int length)
        { }

        protected override void ReceiveRawDatagram(IPEndPoint sender, byte[] datagram)
        { }
    }

    internal struct RemotePeer
    {
        public int Id => EndPoint.GetHashCode();
        public IPEndPoint EndPoint;
        public PeerState State;
    }
}