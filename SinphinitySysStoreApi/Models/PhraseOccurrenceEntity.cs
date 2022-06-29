using Sinphinity.Models;

namespace SinphinitySysStore.Models
{
    public class PhraseOccurrenceEntity
    {
        public PhraseOccurrenceEntity() { }

        public PhraseOccurrenceEntity(Sinphinity.Models.SongLocation o, long phraseId)
        {
            SongId = o.SongId;
            PhraseId = phraseId;
            Voice = o.Voice;
            BarNumber = o.BarNumber;
            Beat = o.Beat;
            StartTick = o.StartTick;
            EndTick = o.EndTick;
        }

        public PhraseOccurrence AsPhraseOccurrence()
        {        
            return new PhraseOccurrence
            {
                BarNumber = this.BarNumber,
                Beat = this.Beat,
                StartTick = this.StartTick,
                EndTick = this.EndTick,
                Voice = this.Voice,
                Song = new Sinphinity.Models.Song { Id = this.SongId }
            };
        }
        public long Id { get; set; }
        public long SongId { get; set; }
        public long PhraseId { get; set; }
        public byte Voice { get; set; }
        public int BarNumber { get; set; }
        public int Beat { get; set; }
        public long StartTick { get; set; }

        public long EndTick { get; set; }
    }
}
