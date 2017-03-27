using System;
using System.Threading.Tasks;
using NUnit.Framework;
using RevScrob;

namespace RSTest
{
    [TestFixture]
    public class BatchProcessorTests
    {
        [Test]
        public async Task RecentTracksTest()
        {
            var batch = new BatchProcessor();
            int i = await Task.Run(() =>  batch.ProcessRecentTracks());

            Console.WriteLine("Processed " + i);
            Assert.Inconclusive();
        }

        [Test, Explicit, Ignore("You need to run this one manually.")]
        public async Task ProcessLibrary()
        {
            var batch = new BatchProcessor();
            int i = await Task.Run(() => batch.ProcessLibrary());

            Console.WriteLine("Processed " + i);
            Assert.Inconclusive();
        }
    }
}