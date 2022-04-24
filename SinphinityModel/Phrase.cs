using System.ComponentModel.DataAnnotations.Schema;

namespace Sinphinity.Models
{
    public class Phrase
    {
        public long Id { get; set; }

        public PhraseMetrics PhraseMetrics { get; set; }

        public PhrasePitches PhrasePitches { get; set; }

        public Phrase() { }

        public Phrase(string asString, long? id = null)
        {
            var parts = asString.Split('/');
            PhraseMetrics = new PhraseMetrics(parts[0]);
            PhrasePitches = new PhrasePitches(parts[1]);
            Id = Id;
            AsString = asString;
        }
        public Phrase(PhraseMetrics phraseMetrics, PhrasePitches phrasePitches)
        {
            PhraseMetrics = phraseMetrics;
            PhrasePitches = phrasePitches;
            AsString = $"{phraseMetrics.AsString}/{phrasePitches.AsString}";
        }

        public string AsString { get; set; }
        public long DurationInTicks { get; set; }
        public int NumberOfNotes { get; set; }
        public int Range { get; set; }
        public int Step { get; set; }
        public bool IsMonotone { get; set; }

        public PhraseTypeEnum PhraseType
        {
            get
            {
                return PhraseTypeEnum.Both;
            }
        }

    }
}
