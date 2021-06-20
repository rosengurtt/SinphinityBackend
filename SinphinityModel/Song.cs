
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinphinity.Models
{
    public class Song
    {
        public virtual string Id { get; set; }
        public string Name { get; set; }
        public string MidiBase64Encoded { get; set; }
        public bool IsSongProcessed { get; set; }
        public bool IsMidiCorrect { get; set; }

        public bool CantBeProcessed { get; set; }
        public Band Band { get; set; }
        public Style Style { get; set; }
        public MidiStats MidiStats { get; set; }
      
        public List<SongSimplification> SongSimplifications { get; set; }
        public List<Bar> Bars { get; set; }
        public List<TempoChange> TempoChanges { get; set; }
        public long DurationInSeconds { get; set; }
        public long DurationInTicks { get; set; }
        public long AverageTempoInBeatsPerMinute { get; set; }
    }
}
