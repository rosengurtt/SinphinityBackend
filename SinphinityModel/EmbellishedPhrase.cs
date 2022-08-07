using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Sinphinity.Models
{
    public class EmbellishedPhrase
    {
        public EmbellishedPhrase() { }
        /// <summary>
        /// The string mus use a "|" to separate the embellished part from the non embellished part
        /// Each part uses a "/" to separate the metrics from the pitches
        /// </summary>
        /// <param name="p"></param>
        public EmbellishedPhrase(string p, long? id = null)
        {
            var parts = p.Split('|');
            var embellishedParts = parts[0].Split('/');
            var nonEmbelishedParts = parts[1].Split('/');
            EmbellishedPhraseMetrics = new EmbellishedPhraseMetrics(nonEmbelishedParts[0], embellishedParts[0]);
            EmbellishedPhrasePitches = new EmbellishedPhrasePitches(nonEmbelishedParts[1], embellishedParts[1]);
            AsStringWithoutOrnaments = $"{EmbellishedPhraseMetrics.AsStringWithoutOrnaments}/{EmbellishedPhrasePitches.AsStringWithoutOrnaments}";
            AsString = $"{EmbellishedPhraseMetrics.AsString}/{EmbellishedPhrasePitches.AsString}";
            Id = id == null ? 0 : (long)id;
        }
        public EmbellishedPhrase(EmbellishedPhraseMetrics embellishedPhraseMetrics, EmbellishedPhrasePitches embellishedPhrasePitches, long? id)
        {
            EmbellishedPhraseMetrics = embellishedPhraseMetrics;
            EmbellishedPhrasePitches = embellishedPhrasePitches;
            AsStringWithoutOrnaments = $"{EmbellishedPhraseMetrics.AsStringWithoutOrnaments}/{EmbellishedPhrasePitches.AsStringWithoutOrnaments}";
            AsString = $"{EmbellishedPhraseMetrics.AsString}/{EmbellishedPhrasePitches.AsString}";
            Id = id == null ? 0 : (long)id;

        }
        public EmbellishedPhrase(string asStringWithoutEmbellishments, string asString, long? id = null)
        {
            var partsWithout = asStringWithoutEmbellishments.Split('/');
            var partsWith = asString.Split('/');
            EmbellishedPhraseMetrics = new EmbellishedPhraseMetrics(partsWithout[0], partsWith[0]);
            EmbellishedPhrasePitches = new EmbellishedPhrasePitches(partsWithout[1], partsWith[1]);
            AsStringWithoutOrnaments = $"{EmbellishedPhraseMetrics.AsStringWithoutOrnaments}/{EmbellishedPhrasePitches.AsStringWithoutOrnaments}";
            AsString = $"{EmbellishedPhraseMetrics.AsString}/{EmbellishedPhrasePitches.AsString}";
            Id = id == null ? 0 : (long)id;
        }


        public long Id { get; set; }

        public EmbellishedPhraseMetrics EmbellishedPhraseMetrics { get; set; }

        public EmbellishedPhrasePitches EmbellishedPhrasePitches { get; set; }

        public string AsStringWithoutOrnaments { get; set; }
        public string AsStringWithoutOrnamentsAccum
        {
            get
            {
                var parts = AsStringWithoutOrnaments.Split('/');
                var phraseMetrics = new PhraseMetrics(parts[0]);
                var phrasePitches = new PhrasePitches(parts[1]);

                return $"{phraseMetrics.AsStringAccum}/{phrasePitches.AsStringAccum}";
            }
        }

        public string AsString { get; set; }
        /// <summary>
        /// Similar to AsString, but instead of having relative pitches and ticks, all pitches and ticks are relative to the first one.
        /// If AsString is 
        /// 24,48,24,96/2,1,-3,4
        /// AsStringAccum is
        /// 24,72,96,192/2,3,0,4
        /// </summary>
        public string AsStringAccum
        {
            get
            {
                if (EmbellishedPhraseMetrics == null || EmbellishedPhrasePitches == null)
                {
                    var partsWithout = AsStringWithoutOrnaments.Split('/');
                    var partsWith = AsString.Split('/');
                    EmbellishedPhraseMetrics = new EmbellishedPhraseMetrics(partsWithout[0], partsWith[0]);
                    EmbellishedPhrasePitches = new EmbellishedPhrasePitches(partsWithout[1], partsWith[1]);
                }
                return $"{EmbellishedPhraseMetrics.AsStringAccum}/{EmbellishedPhrasePitches.AsStringAccum}";
            }
        }
        public string AsStringBasic { get; set; }
        /// <summary>
        /// When we have something link C,D,C,E,C,F, this is equivalent to D,E,D,F,D,G (is the same pattern transposed up by 2 semitones). But they are not exactly the same
        /// according to our definition of a phrasePitches, because the first would be coded as 2,-2,4,-4,5 and the second as 2,-2,3,-3,4
        /// We use the "PhraseDistance" to compare the 2 and if it is small enough we consider the 2 phrases equivalent a we create only 1 record in the db. The AsString value is set
        /// to one of the instances, and we store all equivalent phrases in the Equivalence field. In the database, this is stored as a json object in 1 varchar column, rather than
        /// creating an extra table
        /// 
        /// We only care about pitches for equivalences. the metrics part must be the same in the 2 phrases
        /// </summary>
        public List<string> Equivalences { get; set; }
        public long DurationInTicks { get; set; }
        public int NumberOfNotes { get; set; }
        public int Range { get; set; }
        public int Step { get; set; }
        public bool IsMonotone { get; set; }

        public PhraseTypeEnum PhraseType
        {
            get
            {
                return PhraseTypeEnum.EmbellishedBoth;
            }
        }
        [JsonIgnore]
        public Phrase AsPhrase
        {
            get
            {
                var parts = AsString.Split("/");
                var phraseMetrics = new PhraseMetrics(parts[0]);
                var phrasePitches = new PhrasePitches(parts[1]);
                return new Phrase(phraseMetrics, phrasePitches);
            }
        }

        public Song AsSong
        {
            get
            {
                var asPhrase = new Phrase(AsString);
                return asPhrase.AsSong;
            }
        }
    }
}
