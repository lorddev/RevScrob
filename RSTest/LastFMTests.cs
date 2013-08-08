using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using RevScrob;
using iTunesLib;
using Moq;

namespace RSTest
{
    [TestFixture]
    public class LastFMTests
    {
        [Test]
        public void CanLoadLastFMSingleMock()
        {
            var mock = new Mock<ILastFM>();
            var mock2 = new Mock<IRevTrack>();
            mock2.SetupGet(x => x.PlayCount).Returns(50);
            IRevTrack track = mock2.Object;

            mock.Setup(m => m.GetTrack("Billy Bragg", "To Have and to Have Not")).Returns(track);

            var lib = mock.Object;
            var track2 = lib.GetTrack("Billy Bragg", "To Have and to Have Not");
            Assert.IsNotNull(track2);
            Assert.That(track2.PlayCount == 50);
        }

        [Test]
        public void CanLoadLastFMSingle()
        {
            var lib = new LastFMLibrary();
            var track = lib.GetTrack("Billy Bragg", "To Have and to Have Not");
            Assert.IsNotNull(track);
            Assert.That(track.PlayCount == 50);
        }

        [Test]
        public void CanLoadLastFMMultiple()
        {
            var lib = new Mock<ILastFM>().Object;
            Assert.IsNotNull(lib.GetRecentTracks("alord1647fm"));
        }
    }
}