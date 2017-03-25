using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using iTunesLib;
using Microsoft.CSharp.RuntimeBinder;
using RevScrob.Properties;

#pragma warning disable S1449 // Culture should be specified for "string" operations

namespace RevScrob
{
    public class BatchProcessor
    {
        public async Task<int> ProcessRecentTracks()
        {
            int processed = 0;
            var lib = new LastFMLibrary();
            try
            {
                using (var app = new iTunesLibrary())
                {
                    var itunes = app.GetLibraryAsDictionary();

                    // Process 10 pages
                    for (int i = 1; i <= 10; i++)
                    {
                        // Start with the list of recent tracks, which includes last played date
                        // Make a call to track.getInfo to get the total user play count
                        var tracks = await lib.GetRecentTracks(Settings.Default.LastFMUser, i, 200);

                        if (tracks == null)
                        {
                            break;
                        }

                        foreach (var t2 in tracks.OrderByDescending(x => x.PlayDate))
                        {
                            IITTrack track;
                            IRevTrack playCount;
                            DateTime? playDate = t2.PlayDate;
                            
                            try
                            {
                                track =
                                    itunes[iTunesLibrary.SelectKey(t2.Artist, t2.Album ?? string.Empty, t2.Song)];
                            }
                            catch (KeyNotFoundException)
                            {
                                var orig = Console.ForegroundColor;
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine(
                                    $"Need to buy? {t2.Album} {t2.Song} by {t2.Artist}");
                                Console.ForegroundColor = orig;
                                continue;
                            }

                            if (playDate != null && TrackRecord.Instance.IsCurrent(
                                    track.Name, track.Album, playDate.Value.ToUniversalTime()))
                            {
                                Console.WriteLine("I listen to this song a lot...");
                                continue;
                            }

                            try
                            {
                                if (t2.Song.Length > 36 && t2.MBId.Length == 36)
                                {
                                    playCount = await lib.GetTrackAsync(t2.MBId);
                                }
                                else
                                {
                                    playCount = await lib.GetTrackAsync(t2.Artist, t2.Song);
                                }
                            }
                            catch (RuntimeBinderException error)
                            {
                                Console.WriteLine(error.ToString());
                                continue;
                            }
                            
                            UpdateTrackCountAndDate(playCount, track, playDate);
                            processed++;
                        }
                    }
                }
            }
            catch (COMException exception)
            {
                if (exception.Message.Contains("CO_E_SERVER_EXEC_FAILURE"))
                {
                    Console.WriteLine("Server busy error from iTunes.");
                }
            }
       
            return processed;
        }

        private static void UpdateTrackCountAndDate(IRevTrack playCount, IITTrack track, DateTime? playDate)
        {
            if (playCount.PlayCount > track.PlayedCount)
            {
                track.PlayedCount = playCount.PlayCount.Value;
            }

            bool playDateError = false;
            var itunesPlayDateUtc = new DateTime(2006, 1, 1);
            try
            {
                itunesPlayDateUtc = track.PlayedDate.ToUniversalTime();
            }
            catch (Exception e)
            {
                playDateError = true;
                Console.WriteLine("Play date error: " + e);
            }

            if (playDateError || playDate.GetValueOrDefault().ToUniversalTime() > itunesPlayDateUtc)
            {
                // Weird output error:
                //     The Farmer's Frolic : Last.FM: 11/29/2012 10:35:20 PM; iTunes: 11/29/2012 11:37:55 PM

                Console.WriteLine($"{track.Name} : Last.FM: {playDate.GetValueOrDefault().ToUniversalTime()}; iTunes: {itunesPlayDateUtc}");

                // The Getter for iTunes converts to local, but the setter expects UTC.
                track.PlayedDate = playDate.GetValueOrDefault().ToUniversalTime();
            }

            TrackRecord.Instance.Set(new RTrack
            {
                Album = track.Album,
                Artist = track.Artist,
                Song = track.Name,
                PlayCount = track.PlayedCount,
                PlayDate = track.PlayedDate
            }, (playDate ?? DateTime.Now).ToUniversalTime());
        }

        public async Task<int> ProcessLibrary()
        {
            int processed = 0;
            var lib = new LastFMLibrary();
            try
            {
                using (var app = new iTunesLibrary())
                {
                    var itunes = app.GetLibraryWithoutDuplicates().OrderBy(x => x.PlayedDate);
                    foreach (IITTrack track in itunes)
                    {
                        Console.WriteLine("iTunes: " + track.Name + " by " + track.Artist);

                        IRevTrack playCount;
                        IRevTrack playDate;

                        // Call user.getArtistTracks to get last played time
                        try
                        {
                            var artistScrobbles = await lib.GetArtistScrobbles(track.Artist, Settings.Default.LastFMUser);
                            playDate = artistScrobbles.OrderByDescending(p => p.PlayDate)
                                .FirstOrDefault(t => string.Equals(t.Song, track.Name,
                                    StringComparison.CurrentCultureIgnoreCase));
                        }
                        catch (RuntimeBinderException error)
                        {
                            Console.WriteLine(error.ToString());
                            continue;
                        }

                        // Check if this is a duplicate.
                        if (playDate != null && TrackRecord.Instance.IsCurrent(
                                track.Name, track.Album, playDate.PlayDate.GetValueOrDefault().ToUniversalTime()))
                        {
                            Console.WriteLine("I listen to this song a lot...");
                            continue;
                        }

                        // Make a call to track.getInfo to get the total user play count
                        try
                        {
                            playCount = await lib.GetTrackAsync(track.Artist, track.Name);
                        }
                        catch (RuntimeBinderException error)
                        {
                            Console.WriteLine(error.ToString());
                            continue;
                        }

                        if (playCount == null) continue;

                        UpdateTrackCountAndDate(playCount, track, playDate?.PlayDate);

                        processed++;
                    }
                }
            }
            catch (COMException exception)
            {
                if (exception.Message.Contains("CO_E_SERVER_EXEC_FAILURE"))
                {
                    Console.WriteLine("Server busy error from iTunes.");
                }
            }

            return processed;
        }
    }
}