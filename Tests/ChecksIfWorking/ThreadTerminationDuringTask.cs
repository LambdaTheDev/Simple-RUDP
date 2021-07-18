using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Tests.ChecksIfWorking
{
    [TestFixture]
    public class ThreadTerminationDuringTask
    {
        private bool _wasAborted;
        
        [Test]
        public async Task TestThreadTermination()
        {
            Thread thread = new Thread(() =>
            {
                TestedTask();
            });

            thread.IsBackground = true;
            thread.Start();

            await Task.Delay(2000);
            thread.Abort();
            await Task.Delay(100);
            
            Assert.True(_wasAborted);
        }

        private async Task TestedTask()
        {
            try
            {
                await Task.Delay(100000);
            }
            catch (ThreadAbortException e)
            {
                Console.WriteLine("ABORTED");
                _wasAborted = true;
            }
        }
    }
}