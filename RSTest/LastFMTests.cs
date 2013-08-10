using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RevScrob;
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
        public void CanLoadLastFMMultipleMock()
        {
            var mock = new Mock<ILastFM>();

            var mock2 = new Mock<IRevTrack>();
            var mock3 = new Mock<IRevTrack>();

            mock.Setup(m => m.GetRecentTracks("alord1647fm")).Returns(new List<IRevTrack> {mock2.Object, mock3.Object});
            var lib = mock.Object;
            Assert.That(lib.GetRecentTracks("alord1647fm").Count() == 2);
        }

        [Test]
        public void CanLoadLastFMMultiple()
        {
            var lib = new LastFMLibrary();
            var tracks = lib.GetRecentTracks("alord1647fm");

            Assert.IsNotNull(tracks);
            Assert.That(tracks.Count() == 2);

            foreach (var item in tracks)
            {
                Assert.That(item.PlayDate.HasValue);
                Assert.That(item.PlayDate.Value < DateTime.UtcNow);
                Assert.That(item.PlayDate.Value > DateTime.UtcNow.AddDays(-1));
            }

        }
    }
}