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
        public string AsString { get; set; }
        public string AsStringBasic { get; set; }
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
