using Sinphinity.Models;

namespace SinphinitySysStore.Models
{
    public class PhraseOccurrenceEntity
    {
        public PhraseOccurrenceEntity() { }

        public PhraseOccurrenceEntity(Sinphinity.Models.PhraseLocation o, long phraseId, PhraseTypeEnum type)
        {
            SongId = o.SongId;
            PhraseId = phraseId;
            Voice = o.Voice;
            BarNumber = o.BarNumber;
            Beat = o.Beat;
            StartTick = o.StartTick;
            EndTick = o.EndTick;
            StartingPitch = o.StartingPitch;
            Instrument = o.Instrument;
            PhraseType = type;
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
                Song = new Sinphinity.Models.Song { Id = this.SongId },
                PhraseType=this.PhraseType
            };
        }
        public long Id { get; set; }
        public long SongId { get; set; }
        public long PhraseId { get; set; }
        public byte Voice { get; set; }
        public byte Instrument { get; set; }
        public int BarNumber { get; set; }
        public int Beat { get; set; }
        public long StartTick { get; set; }

        public long EndTick { get; set; }
        public int StartingPitch{ get; set; }
        public PhraseTypeEnum PhraseType { get; set; }
    }
}
