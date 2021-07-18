using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleRUDP;
using SimpleRUDP.States;

namespace Tests.PeerTests
{
    [TestFixture]
    public class SimpleConnectionTest
    {
        private Server _server;
        private Client _client;
        
        [SetUp]
        public void SctSetup()
        {
            _server = new Server();
            _client = new Client();
            
            _server.StartServer(12345, 4);
        }

        [Test]
        public async Task ConnectWithAcks()
        {
            ConnectionAttemptState state = await _client.Connect("127.0.0.1", 12345);
            Console.WriteLine(state);
            Assert.True(state == ConnectionAttemptState.Success);
        }
    }
}