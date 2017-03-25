using System;

namespace RevScrob
{
    public interface IRevTrack
    {
        int? PlayCount { get; set; }
        DateTime? PlayDate { get; set; }
        string Song { get; set; }
        string Album { get; set; }
        string Artist { get; set; }
        string MBId { get; set; }
    }
}