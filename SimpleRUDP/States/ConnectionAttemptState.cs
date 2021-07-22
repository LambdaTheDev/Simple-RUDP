namespace SimpleRUDP.States
{
    public enum ConnectionAttemptState : byte
    {
        Connected,
        ServerRejected,
        ServerUnreachable,
        PeerAlreadyUsed,
    }
}