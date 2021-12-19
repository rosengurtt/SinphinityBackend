using Sinphinity.Models;

namespace SinphinitySysStore.Models
{
    public class PatternOccurrence
    {

        public PatternOccurrence() { }

        public PatternOccurrence(Occurrence o, long patternId) {
            SongId = o.SongId;
            PatternId = patternId;
            Voice = o.Voice;
            BarNumber = o.BarNumber;
            Beat = o.Beat;
            Tick = o.Tick;
        }

        public long Id { get; set; }
        public long SongId { get; set; }
        public long PatternId { get; set; }
        public byte Voice { get; set; }
        public long BarNumber { get; set; }
        public long Beat { get; set; }
        public long Tick { get; set; }
    }
}
