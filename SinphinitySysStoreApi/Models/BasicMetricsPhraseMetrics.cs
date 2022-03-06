namespace SinphinitySysStore.Models
{
    /// <summary>
    /// This class is used for the SQL table BasicMetricsPhrasesMetrics, that is a join table between BasicMetrics and PhraseMetrics
    /// </summary>
    public class BasicMetricsPhraseMetrics
    {
        public long Id { get; set; }

        public long PhraseMetricsId { get; set; }
        public long BasicMetricsId { get; set; }
    }
}
