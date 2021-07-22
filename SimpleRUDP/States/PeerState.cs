namespace SimpleRUDP.States
{
    // Represents current state of peer
    public enum PeerState : byte
    {
        Disconnected, // Offline
        Connecting, // Peer has started & wants to connect
        Connected, // Peer has successfully established connection
        Disconnecting, // Peer started disconnecting
    }
}