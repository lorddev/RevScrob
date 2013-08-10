using System;
using NUnit.Framework;
using RevScrob;
using Moq;

namespace RSTest
{
    [TestFixture]
    public class TrackRecordTests
    {
        [Test]
        public void SetTest()
        {
            var mock = new Mock<IRevTrack>();
            Assert.DoesNotThrow(() => TrackRecord.Instance.Set(mock.Object, DateTime.Now));
        }

        [Test]
        public void ContainsTest()
        {
            var mock = new Mock<IRevTrack>();
            string title = "The Ballad of Rachel Corrie";
            string album = "Fight Songs";
            mock.SetupGet(t => t.Song).Returns(title);
            mock.SetupGet(t => t.Album).Returns(album);

            TrackRecord.Instance.Set(mock.Object, DateTime.Today);
            Assert.That(TrackRecord.Instance.Contains(title, album));
        }

        [Test]
        public void IsCurrentTest()
        {
            var mock = new Mock<IRevTrack>();
            string title = "The Ballad of Rachel Corrie";
            string album = "Fight Songs";
            mock.SetupGet(t => t.Song).Returns(title);
            mock.SetupGet(t => t.Album).Returns(album);

            TrackRecord.Instance.Set(mock.Object, DateTime.Today);
            Assert.That(TrackRecord.Instance.IsCurrent(title, album, DateTime.Today.AddSeconds(-1)));
            Assert.IsFalse(TrackRecord.Instance.IsCurrent(title, album, DateTime.Today.AddSeconds(1)));
        }
    }
}