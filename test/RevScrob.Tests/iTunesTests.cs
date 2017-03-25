using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using RevScrob;
using iTunesLib;

namespace RSTest
{
    [TestFixture]
    public class iTunesTests
    {
        [Test]
        public void CanLoadLastFM()
        {

        }


        [Test]
        public void CanLoadTracks()
        {
            using (var app = new iTunesLibrary())
            {
                var lib = app.GetLibraryAsDictionary();
                foreach (var track in lib.Values)
                //for (int i = 1; i < lib.Count(); i++)
                {
                    //IITTrack track = lib[i];
                    Assert.IsNotNull(track);
                    Debug.WriteLine(track.Name);
                    Debug.WriteLine(track.PlayedCount);

                    if (track.Name == "Paint It, Black")
                    {
                        int count = track.PlayedCount;
                        track.PlayedCount++;

                        Assert.That(track.PlayedCount == count + 1);

                        track.PlayedCount--;
                        Assert.That(track.PlayedCount == count);

                        return;
                    }
                }
            }
        }

        //public void Main()
        //{
        //    var mtrack = new Mock<IITTrack>();

        //    var ttrack = mtrack.Object;
        //    ttrack.PlayedCount++;
        //    ttrack.PlayedDate = ttrack.PlayedDate.AddDays(1);

        //    foreach (var track in _itunes.LibraryPlaylist.Tracks)
        //    {
        //        //track.
        //    }

        //}

    }
}
