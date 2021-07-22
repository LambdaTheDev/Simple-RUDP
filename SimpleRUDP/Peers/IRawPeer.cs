using System;
using System.Net;
using SimpleRUDP.Channels;
using SimpleRUDP.States;

namespace SimpleRUDP.Peers
{
    public interface IRawPeer
    {
        Channel[] Channels { get; }
        
        PeerState State { get; } // Current Peer state
        IPEndPoint TargetEndPoint { get; } // EndPoint associated to this Peer

        void OnRawDataReceived(byte[] datagram, IPEndPoint sender); // Action when raw data arrived
        void SendRawDatagram(byte[] datagram, int length, IPEndPoint target); // Method used to send raw data FROM this peer
        void HandleDataPacket(byte[] buffer, int offset, int length, IPEndPoint sender);
        void HandleInternalPacket(byte[] buffer, int offset, int length, IPEndPoint sender);
    }
}