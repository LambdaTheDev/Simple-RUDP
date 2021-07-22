namespace SimpleRUDP.Protocol
{
    public enum PacketId : byte
    {
        Empty = 0, // Empty packet, used to break listening loop
        ClientHandshakeAction, // Client sends this packet when they want to start connection, or ack server's response
        ServerHandshakeAction, // Server sends this packet, when they received client handshake - server includes response in this packet
        SystemPacketAck, // Peers send this packet with packetId, when they received a packet, most commonly used by Reliable Unordered Unefficient channel
        ChannelPacket, // This packet is sent to identify packets sent by channels, 2nd byte is a channel ID,
        DataPacket, // This packet means that datagram only transports data
    }
}