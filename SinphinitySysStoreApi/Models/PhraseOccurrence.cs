using Sinphinity.Models;

namespace SinphinitySysStore.Models
{
    public class PhraseOccurrence
    {
        public PhraseOccurrence() { }

        public PhraseOccurrence(SongLocation o, long phraseId, PhraseTypeEnum phraseType)
        {
            SongId = o.SongId;
            PhraseId = phraseId;
            Voice = o.Voice;
            BarNumber = o.BarNumber;
            Beat = o.Beat;
            Tick = o.Tick;
            PhraseType = phraseType;
        }

        public long Id { get; set; }
        public long SongId { get; set; }
        public long PhraseId { get; set; }
        public byte Voice { get; set; }
        public int BarNumber { get; set; }
        public int Beat { get; set; }
        public long Tick { get; set; }

        public PhraseTypeEnum PhraseType { get; set; }
    }
}
