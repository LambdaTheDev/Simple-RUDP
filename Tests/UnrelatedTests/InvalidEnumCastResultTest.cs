using System;
using NUnit.Framework;

namespace Tests.UnrelatedTests
{
    [TestFixture]
    public class InvalidEnumCastResultTest
    {
        [Test]
        public void WhatHappensIfEnumCastIsInvalid()
        {
            TestEnum test = (TestEnum) 4;
            Console.WriteLine(test);

            TestEnum test2 = (TestEnum) 3;
            Console.WriteLine(test2);
        }

        private enum TestEnum
        {
            Value1 = 1,
            Value2 = 2,
            Value3 = 3
        }
    }
}