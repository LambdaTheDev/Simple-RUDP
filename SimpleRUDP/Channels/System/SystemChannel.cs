using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using SimpleRUDP.Peers;
using SimpleRUDP.Protocol;
using SimpleRUDP.States;

namespace SimpleRUDP.Channels.System
{
    // Reliable Unordered Unefficient channel, used by system to send connect/disconnect packets etc
    public class SystemChannel : Channel
    {
        // Channel settings
        private const int RetryAttempts = 10; // How much times channel should try to deliver a message
        private const int DelayBetweenAckCheck = 200; // How much MS wait between checking for ack packet

        public override int ChannelHeaderSize { get; } = 2;

        private readonly Dictionary<ushort, PacketInstance> _unackedPackets = new Dictionary<ushort, PacketInstance>();
        private ushort _nextPacketId;

        public SystemChannel(IRawPeer boundPeer) : base(boundPeer)
        {
            // Starts a packet sending task
            Task.Run(async () =>
            {
                while (BoundPeer.State != PeerState.Disconnected)
                {
                    await Task.Delay(DelayBetweenAckCheck);
                    if(_unackedPackets.Count == 0) continue;

                    // Loops through all unacked packets & resends them, or cancel,
                    // if too much attempts
                    foreach (var unackedPacket in _unackedPackets)
                    {
                        BoundPeer.SendRawDatagram(unackedPacket.Value.Datagram, unackedPacket.Value.Datagram.Length, unackedPacket.Value.Receiver);
                        unackedPacket.Value.Attempts++;

                        if (unackedPacket.Value.Attempts > RetryAttempts)
                            _unackedPackets.Remove(unackedPacket.Key);
                        
                        
                        //
                        // PacketInstance packet = unackedPacket.Value;
                        // BoundPeer.SendRawDatagram(packet.Datagram, packet.Datagram.Length, packet.Receiver);
                        // packet.Attempts++;
                        //
                        // if (packet.Attempts > RetryAttempts)
                        //     _unackedPackets.Remove(unackedPacket.Key);
                    }
                }
            });
        }

        
        public override void Send(byte[] data, int length, IPEndPoint target)
        {
            // Yes allocations. But I wanna save this datagram for reliability system
            // In true reliable channel I'm gonna think about more effective solution
            byte[] allocDatagram = new byte[1 + ChannelHeaderSize + length];
            allocDatagram[0] = (byte) PacketId.ChannelPacket;
            allocDatagram[1] = (byte) ChannelId.ReliableUnorderedUnefficient;
            allocDatagram[2] = (byte) PacketId.DataPacket;
            global::System.Buffer.BlockCopy(data, 0, allocDatagram, 3, length);
            
            PacketInstance packet = new PacketInstance
            {
                Datagram = allocDatagram,
                Receiver = target
            };
            
            _unackedPackets.Add(_nextPacketId++, packet);
        }

        // Sends specific packet instead of data
        public void SendPacket(byte[] data, int length, PacketId packet, IPEndPoint target)
        {
            // Yes allocations. But I wanna save this datagram for reliability system
            // In true reliable channel I'm gonna think about more effective solution
            byte[] allocDatagram = new byte[1 + ChannelHeaderSize + length];
            allocDatagram[0] = (byte) PacketId.ChannelPacket;
            allocDatagram[1] = (byte) ChannelId.ReliableUnorderedUnefficient;
            allocDatagram[2] = (byte) packet;
            global::System.Buffer.BlockCopy(data, 0, allocDatagram, 3, length);
            
            PacketInstance packetInstance = new PacketInstance
            {
                Datagram = allocDatagram,
                Receiver = target
            };
            
            _unackedPackets.Add(_nextPacketId++, packetInstance);
        }

        public override void Handle(byte[] data, int offset, IPEndPoint target)
        {
            // And again, here we assume that data is correct, but in production check
            // if everything is valid!!!!!!!!
            PacketId packet = (PacketId) data[offset];
            
            ushort ackId = BitConverter.ToUInt16(data, offset + 1);
            
            if (packet == PacketId.SystemPacketAck)
            {
                _unackedPackets.Remove(ackId);
                return;
            }
            else if(packet == PacketId.DataPacket)
            {
                BoundPeer.HandleDataPacket(data, offset + 3, data.Length, target);
            }
            else
            {
                BoundPeer.HandleInternalPacket(data, offset + 3, data.Length, target);
            }
            
            AckPacket(ackId, target);
        }

        
        // Sends ack packets
        private void AckPacket(ushort ackId, IPEndPoint sender)
        {
            // Yeah, I create here a new byte instead of using a channel buffer,
            // But this may be interrupted by incoming packets
            byte[] ackDatagram = new byte[5];
            ackDatagram[0] = (byte) PacketId.ChannelPacket;
            ackDatagram[1] = (byte) ChannelId.ReliableUnorderedUnefficient;
            ackDatagram[2] = (byte) PacketId.SystemPacketAck;
            ackDatagram[3] = (byte) ackId;
            ackDatagram[4] = (byte) (ackId >> 8);

            // Just sending it RetryAttempts times. Acking the ack will require... a lot
            // Of resources.
            for (byte i = 0; i < RetryAttempts; i++)
            {
                BoundPeer.SendRawDatagram(ackDatagram, 5, sender);
            }
        }
        
        
        // CLASS that represents a packet in this channel
        // I needed it to be class to have a reference type instead of
        // value type (struct). Sending packets with this channel will use a lot of
        // memory, so that's why it's called Reliable Unordered Unefficient
        private class PacketInstance
        {
            public byte[] Datagram;
            public int Attempts;
            public IPEndPoint Receiver;
        }
    }
}