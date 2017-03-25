using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using iTunesLib;
using Microsoft.CSharp.RuntimeBinder;


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
                    Dictionary<string, IITTrack> itunes = null;

                    // Process 10 pages
                    for (int i = 1; i <= 10; i++)
                    {
                        var tracks = lib.GetRecentTracks("alord1647fm", i, 200);

                        if (tracks == null)
                        {
                            break;
                        }

                        itunes = itunes ?? app.GetLibraryAsDictionary();

                        foreach (var t2 in tracks.OrderByDescending(x => x.PlayDate))
                        {
                            IITTrack track;
                            IRevTrack updated;
                            try
                            {
                                if (t2.Song.Length > 36 && t2.MBId.Length == 36)
                                {
                                    updated = await lib.GetTrackAsync(t2.MBId);
                                }
                                else
                                {
                                    updated = await lib.GetTrackAsync(t2.Artist, t2.Song);
                                }

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
                                        $"Need to buy? {t2.Album} {t2.Song} by {t2.Artist}, scrobbled {updated.PlayCount} times");
                                    Console.ForegroundColor = orig;
                                    continue;
                                }

                                if (t2.PlayDate != null && TrackRecord.Instance.IsCurrent(
                                        track.Name, track.Album, t2.PlayDate.Value.ToUniversalTime()))
                                {
                                    Console.WriteLine("I listen to this song a lot...");
                                    continue;
                                }
                            }
                            catch (RuntimeBinderException error)
                            {
                                Console.WriteLine(error.ToString());
                                continue;
                            }
                            
                            if (updated.PlayCount > track.PlayedCount)
                            {
                                track.PlayedCount = updated.PlayCount.Value;
                            }

                            bool playDateError = false;
                            var iTunesPlayDate = new DateTime(2006, 1, 1);
                            try
                            {
                                iTunesPlayDate = track.PlayedDate.ToUniversalTime();
                            }
                            catch (Exception e)
                            {
                                playDateError = true;
                                Console.WriteLine("Play date error: " + e);
                            }

                            Debug.Assert(t2.PlayDate != null, "t2.PlayDate != null");
                            if (playDateError || t2.PlayDate.Value.ToUniversalTime() > iTunesPlayDate)
                            {
                                Console.WriteLine("Last.FM: {0}; iTunes: {1}", t2.PlayDate.Value, track.PlayedDate);

                                // The Getter for iTunes converts to local, but the setter expects UTC.
                                track.PlayedDate = t2.PlayDate.Value.ToUniversalTime();
                            }

                            processed++;
                            TrackRecord.Instance.Set(new RTrack
                            {
                                Album = track.Album,
                                Artist = track.Artist,
                                Song = track.Name,
                                PlayCount = track.PlayedCount,
                                PlayDate = track.PlayedDate
                            }, t2.PlayDate.Value.ToUniversalTime());
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
                        IRevTrack t2;

                        try
                        {
                            t2 = await lib.GetTrackAsync(track.Artist, track.Name);
                        }
                        catch (RuntimeBinderException error)
                        {
                            Console.WriteLine(error.ToString());
                            continue;
                        }

                        if (t2 == null) continue;

                        processed++;

                        if (t2.PlayCount > track.PlayedCount)
                        {
                            track.PlayedCount = t2.PlayCount.Value;
                        }

                        bool playDateError = false;
                        var iTunesPlayDate = new DateTime(2006, 1, 1);
                        try
                        {
                            iTunesPlayDate = track.PlayedDate.ToUniversalTime();
                        }
                        catch (Exception e)
                        {
                            playDateError = true;
                            Console.WriteLine("Play date error: " + e);
                        }
                        
                        if (t2.PlayDate != null && (playDateError || t2.PlayDate.Value.ToUniversalTime() > iTunesPlayDate))
                        {
                            Console.WriteLine("Last.FM: {0}; iTunes: {1}", t2.PlayDate.Value, track.PlayedDate);

                            // The Getter for iTunes converts to local, but the setter expects UTC.
                            track.PlayedDate = t2.PlayDate.Value.ToUniversalTime();
                        }

                        TrackRecord.Instance.Set(new RTrack
                        {
                            Album = track.Album,
                            Artist = track.Artist,
                            Song = track.Name,
                            PlayCount = track.PlayedCount,
                            PlayDate = track.PlayedDate
                        }, track.PlayedDate.ToUniversalTime());
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