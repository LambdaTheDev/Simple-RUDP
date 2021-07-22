using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleRUDP.Peers.Client;
using SimpleRUDP.Peers.Server;
using SimpleRUDP.States;

namespace Tests.PeerTests
{
    [TestFixture]
    public class ClientServerConnectionTest
    {
        private Server _server;
        private Client _client;

        [SetUp]
        public void SetupTest()
        {
            _server = new Server();
            _client = new Client();
            
            _server.StartServer(12345, 4);
        }

        [Test]
        public async Task ConnectClientTest()
        {
            string localhostIp = "127.0.0.1";
            
            ConnectionAttemptState connectionRequest = await _client.Connect(localhostIp, 12345);
            Console.WriteLine("Response: " + connectionRequest);

            await Task.Delay(1000);
            
            Assert.True(connectionRequest == ConnectionAttemptState.Connected);
        }
    }
}