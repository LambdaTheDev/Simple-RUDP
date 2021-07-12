namespace SimpleRUDP.States
{
    public enum PeerState : byte
    {
        Offline,
        Connecting,
        Connected,
        Disconnecting,
    }
}