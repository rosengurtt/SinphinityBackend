using System.ComponentModel.DataAnnotations.Schema;

namespace Sinphinity.Models
{
    public class Phrase
    {
        public long Id { get; set; }
     
        public PhraseMetrics PhraseMetrics { get; set; }
  
        public PhrasePitches PhrasePitches { get; set; }

        public Phrase() { }

        public Phrase(string asString) {
            var parts = asString.Split('/');
            PhraseMetrics = new PhraseMetrics(parts[0]);
            PhrasePitches = new PhrasePitches(parts[1]);
        }
        public Phrase(PhraseMetrics phraseMetrics, PhrasePitches phrasePitches) {
            PhraseMetrics = phraseMetrics;
            PhrasePitches = phrasePitches;
        }

        public string AsString
        {
            get
            {
                return $"{PhraseMetrics}/{PhrasePitches}";
            }
        }


    }
}
