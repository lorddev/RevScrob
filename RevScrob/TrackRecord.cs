using System;
using System.Collections.Generic;
using System.Linq;

namespace RevScrob
{
    internal interface ITrackRecord
    {
        bool Contains(string song, string album);
        bool IsCurrent(string song, string album, DateTime modifiedDateUtc);
    }

    /// <summary>
    /// In-memory cache to keep track of items already processed
    /// </summary>
    class TrackRecord : ITrackRecord
    {
        private class TrackTuple : Tuple<IRevTrack, DateTime>
        {
            public TrackTuple(IRevTrack item1, DateTime item2) : base(item1, item2)
            {
            }

            public IRevTrack Track
            {
                get { return Item1; }
            }

            public DateTime Modified
            {
                get { return Item2; }
            }
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

        internal static TrackRecord Instance { get; private set; }

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
                    _records.Add(new TrackTuple(track, DateTime.UtcNow));
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
