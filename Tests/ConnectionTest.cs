using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Simple_RUDP.Peers;

namespace Tests
{
    [TestFixture]
    public class ConnectionTest
    {
        [Test]
        public async Task ConnectToServerTest()
        {
            Server server = new Server();
            server.Start(12345, 3);
        }
    }
}