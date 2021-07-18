namespace SimpleRUDP.Protocol
{
    public enum PacketId : byte
    {
        ClientHandshakeRequest = 1, // Client sends this packet when they want to establish connection or when they
                                // confirm that ServerHandshakeResponse was received
        ServerHandshakeAck, // Server sends this packet when they got client's request
        ClientHandshakeConfirmation, // Client sends this packet to server, when they got their response
    }
}