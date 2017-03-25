using System.Collections.Generic;

namespace RevScrob
{
    public interface ILastFM
    {
        IRevTrack GetTrack(string artist, string song);

        IEnumerable<IRevTrack> GetRecentTracks(string username, int page = 1, int pageSize = 50);
    }
}