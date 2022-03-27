using System.ComponentModel.DataAnnotations.Schema;

namespace Sinphinity.Models
{
    public class EmbellishedPhrase
    {
        public EmbellishedPhrase() { }
        public EmbellishedPhrase(long embellishedPhraseMetricsId, long embellishedPhrasePitchesId)
        {
            EmbellishedPhraseMetricsId = embellishedPhraseMetricsId;
            EmbellishedPhrasePitchesId = embellishedPhrasePitchesId;

        }
        public long Id { get; set; }
        public long EmbellishedPhraseMetricsId { get; set; }
        public long EmbellishedPhrasePitchesId { get; set; }
        [NotMapped]
        public EmbellishedPhraseMetrics? PhraseMetrics { get; set; }
        [NotMapped]
        public EmbellishedPhrasePitches? PhrasePitches { get; set; }
    }
}
