using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp.RuntimeBinder;

namespace RevScrob
{
    public class BatchProcessor
    {
        public async Task<int> Go()
        {
            int processed = 0;
            var lib = new LastFMLibrary();
            try
            {
                using (var app = new iTunesLibrary())
                {
                    var itunes = app.GetLibrary().ToList().OrderByDescending(x => x.PlayedDate);
                    for (int i = 1; i <= 10; i++)
                    {
                        var tracks = lib.GetRecentTracks("alord1647fm", i).OrderByDescending(x => x.PlayDate.Value).ToList();
                        
                        foreach (var t2 in tracks)
                        {
                            foreach (var track in itunes.Where(x => x.Artist == t2.Artist && x.Name == t2.Song))
                            {
                                if (track == null) continue;

                                if (TrackRecord.Instance.IsCurrent(
                                    track.Name, track.Album, t2.PlayDate.Value.ToUniversalTime()))
                                {
                                    Console.WriteLine("I listen to this song a lot...");
                                    continue;
                                }

                                t2.PlayCount = track.PlayedCount;

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
                                }
                                catch (RuntimeBinderException error)
                                {
                                    continue;
                                }

                                if (updated == null) continue;

                                processed++;

                                if (updated.PlayCount > t2.PlayCount)
                                {
                                    t2.PlayCount = updated.PlayCount;
                                }

                                if (t2.PlayCount > track.PlayedCount)
                                {
                                    track.PlayedCount = t2.PlayCount.Value;
                                }

                                if (t2.PlayDate.Value.ToUniversalTime() >
                                    track.PlayedDate.AddMinutes(5).ToUniversalTime())
                                {
                                    Console.WriteLine("Last.FM: {0}; iTunes: {1}", t2.PlayDate.Value, track.PlayedDate);

                                    // The Getter for iTunes converts to local, but the setter expects UTC.
                                    track.PlayedDate = t2.PlayDate.Value.ToUniversalTime();
                                }

                                TrackRecord.Instance.Set(new LastFMLibrary.RTrack
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