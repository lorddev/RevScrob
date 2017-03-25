#pragma warning disable S101 // Types should be named in camel case

using System;

namespace RevScrob
{
    public struct RTrack : IRevTrack
    {
        public int? PlayCount { get; set; }

        public DateTime? PlayDate { get; set; }
        public string Song { get; set; }
        public string Album { get; set; }
        public string Artist { get; set; }
        public string MBId { get; set; }
    }
}
