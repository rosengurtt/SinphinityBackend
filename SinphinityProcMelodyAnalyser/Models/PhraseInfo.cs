using Sinphinity.Models;

namespace SinphinityProcMelodyAnalyser.Models
{
    public class PhraseInfo
    {
        public string MetricsAsString { get; set; }
        public string PitchesAsString { get; set; }
        public string PhraseAsString
        {
            get
            {
                return $"{MetricsAsString}/{PitchesAsString}";
            }
        }
        public SongLocation Location { get; set; }
        public string EmbellishedMetricsAsString { get; set; }
        public string EmbellishedPitchesAsString { get; set; }
        public string EmbellishedPhraseAsString {
            get
            {
                return $"{EmbellishedMetricsAsString}/{EmbellishedPitchesAsString}";
            }
        }
    }
}
