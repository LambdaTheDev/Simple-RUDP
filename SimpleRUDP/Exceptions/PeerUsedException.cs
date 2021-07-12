using System;

namespace SimpleRUDP.Exceptions
{
    public class PeerUsedException : Exception
    {
        public PeerUsedException() : base("Tried to start Peer that is already used.") { }
    }
}