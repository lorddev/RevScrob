using System;
using System.Collections.Generic;
using System.Linq;

namespace RevScrob
{
    /// <summary>
    /// In-memory cache to keep track of items already processed
    /// </summary>
    /// <remarks>Use case is as follows:
    /// <ul>
    /// <li>When iterating backwards through a paginated "recent tracks" call, need to know that we've already
    /// processed that track. In such a case, modifiedDateUtc should be the played date.</li>
    /// </ul>
    /// </remarks>
    public class TrackRecord : ITrackRecord
    {
        private class TrackTuple : Tuple<IRevTrack, DateTime>
        {
            public TrackTuple(IRevTrack item1, DateTime item2) : base(item1, item2)
            {
            }

            public IRevTrack Track => Item1;

            public DateTime Modified => Item2;
        }

        private readonly object _lock = new object();

        private readonly IList<TrackTuple> _records;

        static TrackRecord()
        {
            Instance = new TrackRecord();
        }

        private TrackRecord()
        {
            _records = new List<TrackTuple>();
        }

        public static TrackRecord Instance { get; private set; }

        public bool Contains(string song, string album)
        {
            return _records.Any(x => x.Track.Song == song && x.Track.Album == album);
        }

        public bool IsCurrent(string song, string album, DateTime modifiedDateUtc)
        {
            return _records.Any(x => x.Track.Song == song && x.Track.Album == album && x.Modified >= modifiedDateUtc);
        }

        public void Set(IRevTrack track, DateTime modifiedDate)
        {
            lock (_lock)
            {
                if (!Contains(track.Song, track.Album))
                {
                    _records.Add(new TrackTuple(track, modifiedDate));
                }
                else if (!IsCurrent(track.Song, track.Album, modifiedDate))
                {
                    for (int i = 0; i < _records.Count; i++)
                    {
                        var record = _records[i];
                        if (record.Track.Song == track.Song && record.Track.Album == track.Album)
                        {
                            _records[i] = new TrackTuple(track, modifiedDate);
                            break;
                        }
                    }
                }
            }
        }
    }
}
