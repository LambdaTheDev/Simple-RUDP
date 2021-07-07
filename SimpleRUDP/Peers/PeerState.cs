namespace Simple_RUDP.Peers
{
    public enum PeerState : byte
    {
        Disconnected,
        Disconnecting,
        Connecting,
        Connected,
    }
}