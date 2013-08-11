using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using RevScrob;
using Moq;

namespace RSTest
{
    [TestFixture]
    public class BatchProcessorTests
    {
        [Test]
        public async void GoTest()
        {
            var batch = new BatchProcessor();
            int i = await Task.Run(() =>  batch.Go());

            Console.WriteLine("Processed " + i);
            Assert.Inconclusive();
        }
    }
}