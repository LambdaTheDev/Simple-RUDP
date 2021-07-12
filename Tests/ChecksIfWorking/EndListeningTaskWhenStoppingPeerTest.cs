using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Tests.ChecksIfWorking
{
    [TestFixture]
    public class EndListeningTaskWhenStoppingPeerTest
    {
        private bool _ready;
        private bool _ended;
        private UdpClient _client;
        
        
        [Test]
        public async Task EndListeningTaskWhenStoppingPee()
        {
            _client = new UdpClient(new IPEndPoint(IPAddress.Any, 54321));
            _ready = true;
            ListeningTask();

            await Task.Delay(100); // Wait to make sure it started
            
            
            _ready = false;
            // Send packet to myself, to exit loop
            //_client.
            
            _client.Close();
            _client.Dispose();

            await Task.Delay(100); // Same reason, as above
            
            Assert.True(_ended);
        }

        private async Task ListeningTask()
        {
            while (_ready)
            {
                UdpReceiveResult udpResult = await _client.ReceiveAsync();
                //InvokeUdpPacket(udpResult.RemoteEndPoint, udpResult.Buffer);
            }
        }
    }
}