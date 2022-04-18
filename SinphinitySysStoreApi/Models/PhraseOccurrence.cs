namespace SinphinitySysStore.Models
{
    public class PhraseOccurrence
    {
        public PhraseOccurrence() { }

        public PhraseOccurrence(Sinphinity.Models.SongLocation o, long phraseId)
        {
            SongId = o.SongId;
            PhraseId = phraseId;
            Voice = o.Voice;
            BarNumber = o.BarNumber;
            Beat = o.Beat;
            Tick = o.Tick;
        }

        public long Id { get; set; }
        public long SongId { get; set; }
        public long PhraseId { get; set; }
        public byte Voice { get; set; }
        public int BarNumber { get; set; }
        public int Beat { get; set; }
        public long Tick { get; set; }

    }
}
