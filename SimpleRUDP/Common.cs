using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Simple_RUDP.Channels;
using Simple_RUDP.Channels.Reliable;
using Simple_RUDP.Channels.SemiReliable;
using Simple_RUDP.Channels.Unreliable;
using Simple_RUDP.Protocol;

namespace Simple_RUDP
{
    public abstract class Common
    {
        // Refs to packet listener and sender.
        protected readonly PacketListener Listener = new PacketListener();
        protected readonly PacketSender Sender = new PacketSender();
        
        // Channel instances for this peer
        protected readonly IChannel[] Channels = new IChannel[3]; // Only 3 channels supported!

        protected UdpClient Udp; // UDP listener
        protected Task ListenerTask; // Listener task

        protected bool IsClient; // Returns true, if this peer is client
        protected bool IsListening;  // Returns true, if this peer is listening
        protected bool IsConnected; // Returns true, if this Udp peer is connected


        protected Common()
        {
            SetupChannels();
        }
        

        // Opens UDP connection for provided IP (targetIp). Server will use IPAddress.Any
        public abstract void Start(IPAddress targetIp, ushort port);
        
        // Stops UDP connection
        public abstract void Stop();

        // Handles packet that arrived from endPoint peer
        protected abstract void HandleIncomingPacket(IPEndPoint endPoint, byte[] datagram);


        // Creates instance of channels. Maybe you want different IDs than built-in ones?
        protected void SetupChannels()
        {
            Channels[(byte) ChannelId.Reliable] = new ReliableChannel();
            Channels[(byte) ChannelId.Unreliable] = new UnreliableChannel();
            Channels[(byte) ChannelId.SemiReliable] = new SemiReliable();
        }
        
        // Starts Task that listens for UDP datagrams
        protected async Task Listen()
        {
            while (IsListening && IsConnected)
            {
                UdpReceiveResult result = await Udp.ReceiveAsync();
                HandleIncomingPacket(result.RemoteEndPoint, result.Buffer);
            }
        }

        protected void Reset()
        {
            IsConnected = false;
            IsListening = false;
            Udp.Close();
        }
    }
}