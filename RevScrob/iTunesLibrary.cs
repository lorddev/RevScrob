using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using iTunesLib;

namespace RevScrob
{      
    /*
     * Step 1: Filter through all tracks played in the last year on Last.fm, using user.getRecentTracks to get last date played.
     *  - call track.getInfo to pull the userplaycount
     *  - update iTunes date played only if off by at least 1 hour.
     *  - keep a list of tracks so as to avoid updating what you've already updated.
     * Step 2: Filter through all iTunes tracks with a date played earlier than the last year, and call track.getInfo
     * to get user's Last.fm play counts for that track.
     * Step 2b: If it fails to find a track record with a userplaycount value, call track.search and iterate through 
     * the results, calling track.getInfo for each result based on mbid guid, until you find one with a userplaycount value.
     * Then 
     * Step 3: Every day, call user.getRecentTracks to keep info up-to-date.
     * 
     * User instructions: Sync your iPod first so as to scrobble those tracks. After installing and running, if Last.fm 
     * for Windows asks to scrobble changed tracks, say no.
     * 
     * Plans for 1.1: Enable dynamic creation of playlist similar to "Most Played Albums last 3 months" list on Spotify.
     */
    // ReSharper disable InconsistentNaming
    public class iTunesLibrary : IDisposable
    // ReSharper restore InconsistentNaming
    {
        iTunesApp _itunes = new iTunesAppClass();

        public IEnumerable<IITTrack> GetLibrary()
        {
            //var mtrack = new Mock<IITTrack>();

            //var ttrack = mtrack.Object;
            //ttrack.PlayedCount++;
            //ttrack.PlayedDate = ttrack.PlayedDate.AddDays(1);
            
            //foreach (var track in _itunes.LibraryPlaylist.Tracks)
            //{
            //    //track.
            //}

            return _itunes.LibraryPlaylist.Tracks.Cast<IITTrack>().Where(
                t => !t.KindAsString.Contains("video") && !t.KindAsString.Equals("iTunes Extras"));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing) return;
            try
            {
                _itunes.Quit();
            }
            catch
            {
                Debug.Write("Unable to quit iTunes while disposing.");
            }
            finally
            {
                Marshal.ReleaseComObject(_itunes);
            }
        }
    }
}
