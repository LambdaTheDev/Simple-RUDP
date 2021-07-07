using System.Net;

namespace Simple_RUDP.Peers
{
    // Representation of remote peer used by server
    public struct RemotePeer
    {
        public int Id => EndPoint.GetHashCode();
        public IPEndPoint EndPoint;
        public PeerState State;
    }
}