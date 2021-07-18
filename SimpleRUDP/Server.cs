using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using SimpleRUDP.Exceptions;
using SimpleRUDP.Protocol;
using SimpleRUDP.States;

namespace SimpleRUDP
{
    public sealed class Server : Peer
    {
        private Dictionary<int, RemotePeer> _connectedPeers;
        private int _maxClients;
        
        public event Action<int> ConnectedToServerEvent; // Called when remote peer connects | args: PeerId
        public event Action<int> DisconnectedFromServerEvent; // Called when remote peer disconnects | args: PeerId
        public event Action<int, byte[], int, int> DataReceivedFromClientEvent; // Called when remote peer sends data | args: PeerId, Buffer, Offset, Length

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
            InvokePeerStarted();
        }

        // Method that stops server peer
        public void StopServer()
        {
            if (State == PeerState.Offline || State == PeerState.Disconnecting) return;
            State = PeerState.Disconnecting;
            InvokePeerStopping();
            
            // todo: Send reliable Disconnection packet to all peers, async operation 
            
            InvokePeerStopped();
        }

        // Throws PeerUsedException if Peer isn't offline (so it's already running)
        private void ThrowIfCantStart()
        {
            if(State != PeerState.Offline)
                throw new PeerUsedException();
        }

        // Sends buffer to peer identified by peerId
        public void Send(int peerId, byte[] buffer, int offset, int length)
        {
            if (_connectedPeers.TryGetValue(peerId, out RemotePeer peer))
            {
                // todo: Pack packet to channels, create headers etc
                SendRawDatagram(peer.EndPoint, buffer, offset, length);
            }
        }

        // Sending raw datagram to receiver. Does it really need to be abstract?
        protected override void SendRawDatagram(IPEndPoint receiver, byte[] datagram, int offset, int length)
        {
            Udp.Send(datagram, length, receiver);
        }

        // Actions when datagram was received 
        protected override void ReceiveRawDatagram(IPEndPoint sender, byte[] datagram)
        {
            Console.WriteLine("RECEIVED DATAGRAM: " + BitConverter.ToString(datagram));
            
            // Rn simple if else, handshake testing proposes
            if (datagram[0] == (byte) PacketId.ClientHandshakeRequest)
            {
                // Yeah, allocations. As mentioned above - just testing proposes, no more :P
                byte[] response = new byte[4];
                response[0] = (byte) PacketId.ServerHandshakeAck; // Header
                response[1] = datagram[1]; // First byte of ID
                response[2] = datagram[2]; // Second one
                response[3] = (byte) (_connectedPeers.Count + 1 > _maxClients ? 0 : 1);
                
                SendRawDatagram(sender, response, 0, 4);
                Console.WriteLine("New client wants to connect with ID: " + BitConverter.ToInt16(datagram, 1));
            }
        }
    }

    internal struct RemotePeer
    {
        public int Id => EndPoint.GetHashCode();
        public IPEndPoint EndPoint;
        public PeerState State;
    }
}