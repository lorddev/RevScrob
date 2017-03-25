using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using RevScrob;
using Moq;
using RevScrob.Properties;

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
        public async Task CanLoadLastFMMultipleMock()
        {
            var mock = new Mock<ILastFM>();

            var mock2 = new Mock<IRevTrack>();
            var mock3 = new Mock<IRevTrack>();

            mock.Setup(m => m.GetRecentTracks(Settings.Default.LastFMUser, 0, 10))
                .Returns(Task.FromResult(new List<IRevTrack> {mock2.Object, mock3.Object}.AsEnumerable()));
            var lib = mock.Object;
            var result = await lib.GetRecentTracks(Settings.Default.LastFMUser, 0, 10);
            Assert.That(result.Count() == 2);
        }

        [Test]
        public async Task CanLoadLastFMMultiple()
        {
            var lib = new LastFMLibrary();
            var tracks = await lib.GetRecentTracks(Settings.Default.LastFMUser, 1, 10);

            Assert.IsNotNull(tracks);
            var revTracks = tracks as IRevTrack[] ?? tracks.ToArray();
            Assert.AreEqual(10, revTracks.Length);

            foreach (var item in revTracks)
            {
                Assert.That(item.PlayDate.HasValue);
                Assert.That(item.PlayDate.Value < DateTime.UtcNow);
                Assert.That(item.PlayDate.Value > DateTime.UtcNow.AddDays(-1));
            }

        }
    }
}