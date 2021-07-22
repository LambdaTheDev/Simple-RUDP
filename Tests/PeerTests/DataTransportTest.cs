using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleRUDP.Channels;
using SimpleRUDP.Peers.Client;
using SimpleRUDP.Peers.Server;

namespace Tests.PeerTests
{
    [TestFixture]
    public class DataTransportTest
    {
        private Server _server;
        private Client _client;

        private bool _connected;
        private bool _serverReceived;
        private byte[] _serverReceivedData;
        
        [SetUp]
        public void Setup()
        {
            _server = new Server();
            _client = new Client();

            _server.DataReceivedFromClientEvent += OnPacketReceived;
            _server.StartServer(54321, 4);
            
            _client.ConnectedToServerEvent += OnConnected;
            _client.Connect("127.0.0.1", 54321);
        }

        void OnConnected()
        {
            _connected = true;
            Console.WriteLine("Connected!");
        }

        void OnPacketReceived(int peerId, byte[] buffer, int offset, int length)
        {
            _serverReceived = true;
            _serverReceivedData = new byte[length - (offset + 1)];

            Buffer.BlockCopy(buffer, offset + 1, _serverReceivedData, 0, length - (offset + 1));
        }

        [Test]
        public async Task SendReliableUnefficientPacket()
        {
            while (!_connected)
            {
                await Task.Delay(100);
            }
            
            Console.WriteLine("Sending a packet...");
            byte[] data = {9, 8, 7, 6};
            _client.Send(data, data.Length, ChannelId.Unreliable);

            await Task.Delay(1000);

            bool condition = _serverReceived && _serverReceivedData.SequenceEqual(data);

            Assert.True(condition);
        }
    }
}