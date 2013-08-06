using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using iTunesLib;

namespace RevScrob
{      
    /*
    * Step 1: Filter through all tracks played in the last year on Last.fm, using user.getRecentTracks, in order 
    * to update play counts AND date played.
    *  - update iTunes date played only if off by at least 1 hour.
    *  - keep a list of tracks so as to avoid updating what you've already updated.
    * Step 2: Filter through all iTunes tracks with a date played earlier than the last year, and call track.getInfo
    * to get user's Last.fm play counts for that track.
    * Step 3: Every day, call user.getRecentTracks to keep info up-to-date.
    * 
    * User instructions: Sync your iPod first so as to scrobble those tracks. After installing and running, if Last.fm 
    * for Windows asks to scrobble changed tracks, say no. 
    */
    public class Test : IDisposable
    {
        iTunesApp _itunes = new iTunesAppClass();



        public void Main()
        {
            var mtrack = new Mock<IITTrack>();

            var ttrack = mtrack.Object;
            ttrack.PlayedCount++;
            ttrack.PlayedDate = ttrack.PlayedDate.AddDays(1);
            
            foreach (var track in _itunes.LibraryPlaylist.Tracks)
            {
                //track.
            }

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _itunes = null;
            }
        }
    }
}
