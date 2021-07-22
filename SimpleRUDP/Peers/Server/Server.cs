using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using SimpleRUDP.Channels;
using SimpleRUDP.Exceptions;
using SimpleRUDP.Peers.Remote;
using SimpleRUDP.Protocol;
using SimpleRUDP.States;

namespace SimpleRUDP.Peers.Server
{
    public sealed partial class Server : LocalPeer
    {
        private Dictionary<int, RemotePeer> _connectedPeers; // Dict that holds all RemotePeers
        private int _maxPeers; // Max peer count
        
        public event Action<int> ConnectedToServerEvent; // Called when remote peer connects | args: PeerId
        public event Action<int> DisconnectedFromServerEvent; // Called when remote peer disconnects | args: PeerId
        public event Action<int, byte[], int, int> DataReceivedFromClientEvent; // Called when remote peer sends data | args: PeerId, Buffer, Offset, Length 


        // Method used to open port & start server
        public void StartServer(ushort port, int maxPeers)
        {
            ThrowIfCantStart();

            _maxPeers = maxPeers;
            _connectedPeers = new Dictionary<int, RemotePeer>(maxPeers);
            
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, port);
            TargetEndPoint = serverEndPoint;
            
            Udp = new UdpClient(TargetEndPoint);
            IsListening = true;
            State = PeerState.Connected;
            
            InvokePeerStarted();
        }

        // Method used to disconnect all & close server peer
        public void StopServer()
        {
            if (State == PeerState.Disconnected || State == PeerState.Disconnecting)
                return;

            State = PeerState.Disconnecting;
            InvokePeerStopping();
            
            // todo: Send reliable disconnection packet
            
            InvokePeerStopped();
        }
        
        // Throws PeerAlreadyUsedException if peer is already used
        private void ThrowIfCantStart()
        {
            if(State != PeerState.Disconnected)
                throw new PeerAlreadyUsedException();
        }

        // Sends buffer to peer (by their peer ID).
        public void Send(int peerId, byte[] buffer, int length, ChannelId channel)
        {
            if (_connectedPeers.TryGetValue(peerId, out RemotePeer peer))
            {
                peer.Channels[(byte) channel].Send(buffer, length, peer.TargetEndPoint);
            }
        }

        // Used to simplify RemotePeer code
        internal UdpClient GetServerUdp()
        {
            return Udp;
        }

        
        
        
        // Here we pass datagram to channels, so they can properly handle them
        public override void OnRawDataReceived(byte[] datagram, IPEndPoint sender)
        {
            if (datagram[0] == (byte) PacketId.ClientHandshakeAction)
            { 
                ProcessHandshake(sender, datagram);
                return;
            }
            
            // In Simple RUDP, not-connection packets must come from channel
            if (datagram[0] != (byte) PacketId.ChannelPacket) return;
            
            // Min packet size is 3 - ChannelPacker header, channel ID & target packet ID
            if (datagram.Length < 3) return;

            // Datagram[1] is a channel ID. So, let's grab a channel & execute stuff there
            // To make code easier, let's just assume that channel ID is correct, but
            // in production ALWAYS validate client input!!!
            byte channelId = datagram[1];
            Channel channel;
            
            // If peer already connected - just pass datagram to their channel
            if (_connectedPeers.TryGetValue(sender.GetHashCode(), out RemotePeer peer))
                channel = peer.Channels[channelId];

            else return;
            
            channel.Handle(datagram, 2, sender);
        }

        public override void SendRawDatagram(byte[] datagram, int length, IPEndPoint target)
        {
            Udp.Send(datagram, length, target);
        }

        public override void InvokeDataReceived(byte[] data, int offset, IRawPeer from)
        {
            // If received not from remote endpoint, then something is wrong
            if (!(from is RemotePeer))
                return;
            
            // Invoke an event
            DataReceivedFromClientEvent?.Invoke(from.TargetEndPoint.GetHashCode(), data, offset, data.Length);
        }
    }
}