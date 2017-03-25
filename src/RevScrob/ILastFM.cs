using System.Collections.Generic;
using System.Threading.Tasks;

namespace RevScrob
{
    public interface ILastFM
    {
        IRevTrack GetTrack(string artist, string song);

        Task<IEnumerable<IRevTrack>> GetRecentTracks(string username, int page = 1, int pageSize = 50);
    }
}