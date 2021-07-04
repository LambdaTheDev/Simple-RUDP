namespace Simple_RUDP.Protocol
{
    // IDs for each packet sent using Simple RUDP
    public enum PacketIds : byte
    {
        ClientWantsToConnect, // Client sends this packet, if they want to connect
        ClientWantsToDisconnect, // Client sends this packet, if they want to disconnect
        ServerConnectionResponse, // Server sends this packet with bool parameter if client got accepted or not
        ServerForceDisconnection, // Server sends this packet, to notify client that they got kicked
        ReliableData, // This packet is sent when peer sends reliable data
        SemiReliableData, // This packet is sent when peer sends semi reliable data
        UnreliableData, // This packet is sent when peer sends unreliable data
        KeepAlive, // (Undecided yet) Server or client sends this packet to make sure that client is alive
    }
}