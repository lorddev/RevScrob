using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;

namespace RevScrob
{
    public interface ILastFM
    {
        IRevTrack GetTrack(string artist, string song);

        IEnumerable<IRevTrack> GetRecentTracks(string username, int i = 1);
    }

    public interface IRevTrack
    {
        int? PlayCount { get; set; }
        DateTime? PlayDate { get; set; }
        string Song { get; set; }
        string Album { get; set; }
        string Artist { get; set; }
        string MBId { get; set; }
    }

    public class LastFMLibrary : ILastFM
    {
        public IRevTrack GetTrack(string artist, string song)
        {
            var caller = new RestCaller
                {
                    Host = "http://ws.audioscrobbler.com/",
                    Action = "2.0/",
                    Method = "GET"
                };

            caller.AddParam("method", "track.getInfo")
                .AddParam("username", "alord1647fm")
                .AddParam("api_key", "e2e16b5513251519bdce400fcd094332")
                .AddParam("format", "json");

            caller.AddParam("artist", HttpUtility.UrlEncode(artist));
            caller.AddParam("track", HttpUtility.UrlEncode(song));

            dynamic result = caller.ExecuteAsync().Result;
            return new RTrack
                {
                    Album = result.Data.track.album["#text"],
                    PlayCount = result.Data.track.userplaycount
                };
        }

        public struct RTrack : IRevTrack
        {
            public int? PlayCount { get; set; }

            public DateTime? PlayDate { get; set; }
            public string Song { get; set; }
            public string Album { get; set; }
            public string Artist { get; set; }
            public string MBId { get; set; }
        }

        public IEnumerable<IRevTrack> GetRecentTracks(string username, int page = 1)
        {
            var caller = new RestCaller
                {
                    Host = "http://ws.audioscrobbler.com/",
                    Action = "2.0/",
                    Method = "GET"
                };

            caller.AddParam("method", "user.getRecentTracks")
                  .AddParam("user", username)
                  .AddParam("api_key", "e2e16b5513251519bdce400fcd094332")
                  .AddParam("page", page.ToString())
                  .AddParam("format", "json"); 
            
            dynamic result = caller.ExecuteAsync().Result;
            //return new RTrack { PlayCount = result.Data.track.userplaycount };

            var list = new List<IRevTrack>();

            DateTime Epoch = new DateTime(1970, 1, 1);

            IEnumerable<JToken> tracks = result.Data.recenttracks.track;

            foreach (dynamic item in tracks)
            {
                string utsString = item.date.uts.Value;
                var uts = double.Parse(utsString);
                if (uts > 0)
                {
                    list.Add(new RTrack 
                        {
                            Album = item.album["#text"],
                            Artist = item.artist["#text"],
                            Song = item.name,
                            PlayDate = Epoch.AddSeconds(uts).ToLocalTime(),
                            MBId = item.mbid
                        });
                }
            }

            return list;
        }

        public async Task<IRevTrack> GetTrackAsync(string artist, string song)
        {
            var caller = new RestCaller
            {
                Host = "http://ws.audioscrobbler.com/",
                Action = "2.0/",
                Method = "GET"
            };

            caller.AddParam("method", "track.getInfo")
                .AddParam("username", "alord1647fm")
                .AddParam("api_key", "e2e16b5513251519bdce400fcd094332")
                .AddParam("format", "json");

            caller.AddParam("artist", HttpUtility.UrlEncode(artist));
            caller.AddParam("track", HttpUtility.UrlEncode(song));

            dynamic result = await caller.ExecuteAsync();
        
            return new RTrack
                {
                    Album = result.Data.track.album["#text"],
                    PlayCount = result.Data.track.userplaycount,
                    MBId = result.Data.track.mbid
                };
        }

        public async Task<IRevTrack> GetTrackAsync(string mbid)
        {
            var caller = new RestCaller
                {
                    Host = "http://ws.audioscrobbler.com/",
                    Action = "2.0/",
                    Method = "GET"
                };

            caller.AddParam("method", "track.getInfo")
                .AddParam("username", "alord1647fm")
                .AddParam("api_key", "e2e16b5513251519bdce400fcd094332")
                .AddParam("format", "json");

            caller.AddParam("mbid", HttpUtility.UrlEncode(mbid));

            dynamic result = await caller.ExecuteAsync();

            return new RTrack
            {
                Album = result.Data.track.album["#text"],
                PlayCount = result.Data.track.userplaycount,
                MBId = result.Data.track.mbid
            };
        }
    }
}
