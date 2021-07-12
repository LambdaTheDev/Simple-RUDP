namespace SimpleRUDP.Protocol
{
    public struct Header
    {
        public ushort SequenceId; // Something like packet Id, used by acknowledging (ACKing) system
        public ushort Ack; // Last Sequence Id that was successfully received
        public uint AckBits; // Stores recent ACKs. If bit N is set, then (Ack - N) has been received for sure
    }
}