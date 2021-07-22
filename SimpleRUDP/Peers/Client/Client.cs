using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading.Tasks;
using SimpleRUDP.Channels;
using SimpleRUDP.Peers.Remote;
using SimpleRUDP.Protocol;
using SimpleRUDP.States;

namespace SimpleRUDP.Peers.Client
{
    public sealed partial class Client : LocalPeer
    {
        private static readonly RNGCryptoServiceProvider Rng = new RNGCryptoServiceProvider();

        private IPEndPoint _serverEndPoint;
        
        public event Action ConnectedToServerEvent; // Called when client successfully connects to server
        public event Action DisconnectedFromServerEvent; // Called when client successfully disconnects from server
        public event Action<byte[], int, int> DataReceivedFromServerEvent; // Called when server sends data | args: Buffer, Offset, Length 

        public async Task<ConnectionAttemptState> Connect(string ip, ushort port)
        {
            if (State != PeerState.Disconnected)
                return ConnectionAttemptState.PeerAlreadyUsed;

            State = PeerState.Connecting;
            
            _serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            Udp = new UdpClient();
            Udp.Connect(_serverEndPoint);
            
            IsListening = true;
            InvokePeerStarted();
            
            ConnectionAttemptState result = await TryToConnect();
            if (result == ConnectionAttemptState.Connected)
            {
                // Set state to connected & invoke connected event
                State = PeerState.Connected;
                ConnectedToServerEvent?.Invoke();
            }

            return result;
        }

        public async Task Disconnect()
        {
            InvokePeerStopping();
            
            // Send reliable packet to server
            
            InvokePeerStopped();
            DisconnectedFromServerEvent?.Invoke();
        }
        
        // Sends buffer to server
        public void Send(byte[] buffer, int length, ChannelId channel)
        {
            Channels[(byte) channel].Send(buffer, length, (IPEndPoint) Udp.Client.RemoteEndPoint);
        }
        
        public override void OnRawDataReceived(byte[] datagram, IPEndPoint sender)
        {
            // In Simple RUDP, not-connection packets must come from channel
            if (datagram[0] != (byte) PacketId.ChannelPacket) return;
            
            // Min packet size is 3 - ChannelPacker header, channel ID & target packet ID
            if (datagram.Length < 3) return;

            // Datagram[1] is a channel ID. So, let's grab a channel & execute stuff there
            // To make code easier, let's just assume that channel ID is correct, but
            // in production ALWAYS validate client input!!!
            Channel channel = Channels[datagram[1]];
         
            channel.Handle(datagram, 2, sender);
        }

        public override void SendRawDatagram(byte[] datagram, int length, IPEndPoint target)
        {
            Udp.Send(datagram, length);
        }

        public override void InvokeDataReceived(byte[] data, int offset, IRawPeer from)
        {
            DataReceivedFromServerEvent?.Invoke(data, offset, data.Length);
        }
    }
}