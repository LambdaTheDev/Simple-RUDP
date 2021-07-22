using System.Net;
using SimpleRUDP.Channels;
using SimpleRUDP.States;

namespace SimpleRUDP.Peers.Remote
{
    public sealed partial class RemotePeer : IRawPeer
    {
        private readonly Server.Server _server;
        
        public Channel[] Channels { get; }

        public PeerState State { get; } = PeerState.Disconnected;
        
        public IPEndPoint TargetEndPoint { get; }

        public RemotePeer(IPEndPoint boundEndPoint, Server.Server server)
        {
            TargetEndPoint = boundEndPoint;
            State = PeerState.Connecting;
            Channels = Channel.SetupChannels(this);
            _server = server;
        }
        
        public void OnRawDataReceived(byte[] datagram, IPEndPoint sender)
        {
            // RemotePeer does not receive packets by itself. Server handles it for it.
        }

        public void SendRawDatagram(byte[] datagram, int length, IPEndPoint target)
        {
            _server.GetServerUdp().Send(datagram, length, target);
        }
    }
}