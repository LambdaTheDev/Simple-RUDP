using System;

namespace Simple_RUDP.Exceptions
{
    public class PeerNotReadyException : Exception
    {
        public PeerNotReadyException(string message) : base(message)
        {
            
        }
    }
}