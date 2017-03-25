using System;

namespace RevScrob
{
    public interface ITrackRecord
    {
        bool Contains(string song, string album);
        bool IsCurrent(string song, string album, DateTime modifiedDateUtc);
        void Set(IRevTrack track, DateTime modifiedDate);
    }
}