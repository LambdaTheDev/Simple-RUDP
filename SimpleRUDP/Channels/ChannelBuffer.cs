namespace SimpleRUDP.Channels
{
    public struct ChannelBuffer
    {
        public byte[] Buffer;
        public int Length;

        public ChannelBuffer(int bufferSize)
        {
            Buffer = new byte[bufferSize];
            Length = 0;
        }
    }
}