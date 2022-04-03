using System.ComponentModel.DataAnnotations.Schema;

namespace Sinphinity.Models
{
    public class EmbellishedPhrase
    {
        /// <summary>
        /// The string mus use a "|" to separate the embellished part from the non embellished part
        /// Each part uses a "/" to separate the metrics from the pitches
        /// </summary>
        /// <param name="p"></param>
        public EmbellishedPhrase(string p)
        {
            var parts = p.Split('|');
            var embellishedParts = parts[0].Split('/');
            var nonEmbelishedParts = parts[1].Split('/');
            EmbellishedPhraseMetrics = new EmbellishedPhraseMetrics(nonEmbelishedParts[0], embellishedParts[0]);
            EmbellishedPhrasePitches = new EmbellishedPhrasePitches(nonEmbelishedParts[1], embellishedParts[1]);



        }
        public EmbellishedPhrase(EmbellishedPhraseMetrics embellishedPhraseMetrics, EmbellishedPhrasePitches embellishedPhrasePitches)
        {
            EmbellishedPhraseMetrics = embellishedPhraseMetrics;
            EmbellishedPhrasePitches = embellishedPhrasePitches;

        }
        public long Id { get; set; }

        public EmbellishedPhraseMetrics EmbellishedPhraseMetrics { get; set; }
    
        public EmbellishedPhrasePitches EmbellishedPhrasePitches { get; set; }

        public string AsStringWithoutOrnaments {
            get
            {
                return $"{EmbellishedPhraseMetrics.AsStringWithoutOrnaments}/{EmbellishedPhrasePitches.AsStringWithoutOrnaments}";
            }
        }
        public string AsString
        {
            get
            {
                return $"{EmbellishedPhraseMetrics.AsString}/{EmbellishedPhrasePitches.AsString}";
            }
        }
    }
}
