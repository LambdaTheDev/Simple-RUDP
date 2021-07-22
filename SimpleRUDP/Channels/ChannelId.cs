namespace SimpleRUDP.Channels
{
    public enum ChannelId
    {
        // RO is 0 & unreliable is 1 to integrate this system with other thing I develop
        ReliableOrdered = 0,
        Unreliable = 1,
        ReliableUnorderedUnefficient
    }
}