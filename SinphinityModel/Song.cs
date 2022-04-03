using System.Collections.Generic;

namespace Sinphinity.Models
{
    public class Song
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string? MidiBase64Encoded { get; set; }
        public bool IsSongProcessed { get; set; }
        public bool ArePhrasesExtracted { get; set; }
        public bool IsMidiCorrect { get; set; }

        public bool CantBeProcessed { get; set; }
        public Band? Band { get; set; }
        public Style? Style { get; set; }
        public MidiStats? MidiStats { get; set; }
      
        public List<SongSimplification>? SongSimplifications { get; set; }
        public List<Bar>? Bars { get; set; }
        public List<TempoChange>? TempoChanges { get; set; }
        public long AverageTempoInBeatsPerMinute { get; set; }
    }
}
