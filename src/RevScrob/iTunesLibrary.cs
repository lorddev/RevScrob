using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using iTunesLib;

#pragma warning disable S1449 // Culture should be specified for "string" operations

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
    {
        readonly iTunesApp _itunes = new iTunesAppClass();

        public IEnumerable<IITTrack> GetLibrary()
        {
            return _itunes.LibraryPlaylist.Tracks.Cast<IITTrack>().Where(
                t => !t.KindAsString.Contains("video") && !t.KindAsString.Equals("iTunes Extras"));
        }

        public IEnumerable<IITTrack> GetLibraryWithoutDuplicates()
        {
            var itunes = new Dictionary<string, IITTrack>();
            var duplicates = new HashSet<string>();
            foreach (IITTrack track in _itunes.LibraryPlaylist.Tracks)
            {
                try
                {
                    if (track.KindAsString != null && !track.KindAsString.Contains("video") &&
                        !track.KindAsString.Equals("iTunes Extras"))
                    {
                        itunes.Add(SelectKey(track.Artist.Split('&')[0].Trim(), track.Album ?? string.Empty, track.Name), track);
                    }
                }
                catch (ArgumentException)
                {
                    duplicates.Add(
                        SelectKey(track.Artist.Split('&')[0].Trim(), track.Album ?? string.Empty, track.Name));
                }
            }

            foreach (var key in duplicates)
            {
                itunes.Remove(key);
            }

            return itunes.Values;
        }

        public Dictionary<string, IITTrack> GetLibraryAsDictionary()
        {
            //var mtrack = new Mock<IITTrack>();

            //var ttrack = mtrack.Object;
            //ttrack.PlayedCount++;
            //ttrack.PlayedDate = ttrack.PlayedDate.AddDays(1);
            
            //foreach (var track in _itunes.LibraryPlaylist.Tracks)
            //{
            //    //track.
            //}
            
            var itunes = new Dictionary<string, IITTrack>();
            foreach (IITTrack track in _itunes.LibraryPlaylist.Tracks)
            {
                try
                {
                    if (track.KindAsString != null && !track.KindAsString.Contains("video") &&
                        !track.KindAsString.Equals("iTunes Extras"))
                    {
                        itunes.Add(SelectKey(track.Artist.Split('&')[0].Trim(), track.Album ?? string.Empty, track.Name), track);
                    }
                }
                catch (ArgumentException)
                {
                    Console.WriteLine("Delete duplicate? " + track.Name);
                }
            }
            
            return itunes;
        }

        public static string SelectKey(string artist, string album, string song)
        {
            // Joe Strummer and the Mescaleros => Joe Strummer
            // Johnny Cash & Fiona Apple => Johnny Cash
            // A Mighty Fortress Is Our God => A Mighty Fortress
            // (F)lannigan's Ball => Flannigan's Ball
            // Deceptively Yours (DJ Strobe 7 Below Zero Remix) => Deceptively Yours DJ Strobe 7 Below Zero Remix
            // Glo in the Dark Part 1
            // Album Title (Remastered) ... 
            artist = artist.ToUpper().Replace(" AND ", "&").Replace(" ", "").Split('&')[0];
            album = album.ToUpper().Replace(" ", "");
            album = album.Split('(')[0];
            song = song.Replace("(", "").Replace(")", "");
            if (!song.ToLower().Contains("mix") && !song.Contains("Reprise")
                && !song.ToLower().Contains("instrumental")
                && !song.ToLower().Contains("feat")
                && !song.ToLower().Contains("version")
                && !song.Contains(":")
                && !song.ToLower().Contains("variatio")
                && !song.ToLower().Contains("no.")
                && !song.Contains("Part") && !song.Contains("Pt.") && !song.Contains("Sc.") && !song.Contains("Acoustic") && song.Length > 15)
            {
                song = song.Substring(0, 15);
            }

            song = song.ToUpper().Replace(" ", "");

            return $"{artist}_{album}_{song}";
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
