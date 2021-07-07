using System;

namespace Simple_RUDP.Exceptions
{
    public class PeerAlreadyUsedException : Exception
    {
        public PeerAlreadyUsedException() : base("You are trying to start peer that is already active!")
        {
            
        }
    }
}