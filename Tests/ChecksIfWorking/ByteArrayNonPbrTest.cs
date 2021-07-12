using NUnit.Framework;

namespace Tests.ChecksIfWorking
{
    [TestFixture]
    public class ByteArrayNonPbrTest
    {
        [Test]
        public void TestIfByteArrayNeedsToBePassByReferenceTest()
        {
            byte[] testedArray = new byte[16];
            testedArray[0] = 16;
            
            TestMethod(testedArray);

            bool condition = testedArray[0] == 16 && testedArray[1] == 32 && testedArray[2] == 64;
            Assert.True(condition);
        }

        private void TestMethod(byte[] array)
        {
            array[1] = 32;
            array[2] = 64;
        }
    }
}