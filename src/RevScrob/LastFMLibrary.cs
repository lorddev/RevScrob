#pragma warning disable S101 // Types should be named in camel case

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;
using RevScrob.Properties;

namespace RevScrob
{
    public partial class LastFMLibrary : ILastFM
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
                .AddParam("username", Settings.Default.LastFMUser)
                .AddParam("api_key", "e2e16b5513251519bdce400fcd094332")
                .AddParam("format", "json");

            caller.AddParam("artist", HttpUtility.UrlEncode(artist));
            caller.AddParam("track", HttpUtility.UrlEncode(song));

            dynamic result = caller.ExecuteAsync().Result;
            return new RTrack
                {
                    Artist = artist,
                    Song = song,
                    Album = result.Data.track.album["#text"],
                    PlayCount = result.Data.track.userplaycount
                };
        }

        public async Task<IEnumerable<IRevTrack>> GetRecentTracks(string username, int page = 1, int pageSize = 50)
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
                  .AddParam("limit", pageSize.ToString())
                  .AddParam("format", "json"); 
            
            dynamic result = await caller.ExecuteAsync();
            //return new RTrack { PlayCount = result.Data.track.userplaycount };

            var list = new List<IRevTrack>();

            DateTime epoch = new DateTime(1970, 1, 1);

            IEnumerable<JToken> tracks = result.Data != null ? result.Data.recenttracks.track : null;

            if (tracks == null)
            {
                return list;
            }
            
            foreach (dynamic item in tracks)
            {
                Console.WriteLine(item.name.Value);

                if (item["@attr"]?["nowplaying"] == "true")
                {
                    continue;
                }

                string utsString = item.date.uts.Value;
                var uts = double.Parse(utsString);
                if (uts > 0)
                {
                    var rtrack = new RTrack
                    {
                        Album = item.album["#text"].Value,
                        Artist = item.artist["#text"].Value,
                        Song = item.name.Value,
                        MBId = item.mbid.Value,
                        PlayDate = epoch.AddSeconds(uts).ToLocalTime()
                    };

                    list.Add(rtrack);
                }
            }

            return list;
        }

        private readonly Dictionary<string, IEnumerable<IRevTrack>> _artistTrackScrobbles = new Dictionary<string, IEnumerable<IRevTrack>>();

        public async Task<IEnumerable<IRevTrack>> GetArtistScrobbles(string artist, string username, int page = 1, int pageSize = 50)
        {
            if (_artistTrackScrobbles.ContainsKey(artist.ToLower()))
            {
                return _artistTrackScrobbles[artist.ToLower()];
            }

            var caller = new RestCaller
            {
                Host = "http://ws.audioscrobbler.com/",
                Action = "2.0/",
                Method = "GET"
            };

            caller.AddParam("method", "user.getArtistTracks")
                .AddParam("user", username)
                .AddParam("artist", artist)
                .AddParam("api_key", "e2e16b5513251519bdce400fcd094332")
                .AddParam("page", page.ToString())
                .AddParam("limit", pageSize.ToString())
                .AddParam("format", "json");

            dynamic result = await caller.ExecuteAsync();
            //return new RTrack { PlayCount = result.Data.track.userplaycount };

            var list = new List<IRevTrack>();

            var epoch = new DateTime(1970, 1, 1);

            IEnumerable<JToken> tracks = result.Data != null ? result.Data.artisttracks.track : null;
            
            if (tracks == null)
            {
                _artistTrackScrobbles.Add(artist.ToLower(), list);
                return list;
            }

            foreach (dynamic item in tracks)
            {
                Console.WriteLine(item.name.Value);
                
                string utsString = item.date.uts.Value;
                var uts = double.Parse(utsString);
                if (uts > 0)
                {
                    var rtrack = new RTrack
                    {
                        Album = item.album["#text"].Value,
                        Artist = item.artist["#text"].Value,
                        Song = item.name.Value,
                        PlayDate = epoch.AddSeconds(uts).ToLocalTime(),
                        MBId = item.mbid.Value
                    };
                    list.Add(rtrack);
                }
            }

            _artistTrackScrobbles.Add(artist.ToLower(), list);

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
                .AddParam("username", Settings.Default.LastFMUser)
                .AddParam("api_key", "e2e16b5513251519bdce400fcd094332")
                .AddParam("format", "json");

            caller.AddParam("artist", HttpUtility.UrlEncode(artist));
            caller.AddParam("track", HttpUtility.UrlEncode(song));

            dynamic result = await caller.ExecuteAsync();

            if (result.Error != null && result.Error.message == "Track not found")
            {
                return null;
            }

            var rtrack = new RTrack();
            //try
            {
                dynamic track = result.Data != null ? result.Data.track : null;
                if (track == null)
                {
                    return null;
                }
               
                rtrack.PlayCount = track.userplaycount;
                rtrack.MBId = track.mbid;
                rtrack.Album = track.album?.title;

                return rtrack;
            }
           // catch (RuntimeBinderException)
            {
                //if (!Debugger.IsAttached)
                //{
                //    Debugger.Launch();
                //}

                //throw;
            }
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
                .AddParam("username", Settings.Default.LastFMUser)
                .AddParam("api_key", "e2e16b5513251519bdce400fcd094332")
                .AddParam("format", "json");

            caller.AddParam("mbid", HttpUtility.UrlEncode(mbid));

            dynamic result = await caller.ExecuteAsync();

            return new RTrack
            {
                Album = result.Data.track.album?.title,
                PlayCount = result.Data.track.userplaycount,
                MBId = result.Data.track.mbid
            };
        }
    }
}
