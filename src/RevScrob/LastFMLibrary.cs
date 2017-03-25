#pragma warning disable S101 // Types should be named in camel case

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json.Linq;

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

        public IEnumerable<IRevTrack> GetRecentTracks(string username, int page = 1, int pageSize = 50)
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
            
            dynamic result = caller.ExecuteAsync().Result;
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
                string utsString = item.date.uts.Value;
                var uts = double.Parse(utsString);
                if (uts > 0)
                {
                    list.Add(new RTrack
                    {
                        Album = item.album["#text"],
                        Artist = item.artist["#text"],
                        Song = item.name,
                        PlayDate = epoch.AddSeconds(uts).ToLocalTime(),
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

            if (result.Error != null && result.Error.message == "Track not found")
            {
                return null;
            }

            var track = new RTrack();
            //try
            {
                var data = result.Data;
                track.PlayCount = data.track.userplaycount;
                track.MBId = data.track.mbid;
                track.Album = data.album?.title;

                return track;
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
                .AddParam("username", "alord1647fm")
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
