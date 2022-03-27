namespace Sinphinity.Models
{
    /// <summary>
    /// When the original version of a phrase has embellishments we create 2 objects
    /// 
    /// - the original phrase with the embellishments (that we store in this type of object)
    /// - the simplified version of the phrase with the embellisments removed (that we store in a PhraseMetrics object, that we link from here)
    /// 
    /// 
    /// </summary>
    public class EmbellishedPhraseMetrics : PhraseMetrics
    {
        public EmbellishedPhraseMetrics(PhraseMetrics p, string asString)
        {
            PhraseMetricsWithoutOrnamentsId = p.Id;
            AsStringWithoutOrnaments = p.AsString;
            AsString = asString;
            BasicMetricId = p.BasicMetricId;
        }
        /// <summary>
        /// Link to the version of the phrase with the ornaments removed
        /// </summary>
        public long PhraseMetricsWithoutOrnamentsId { get; set; }
        /// <summary>
        /// The AsString of the version without ornaments
        /// </summary>
        public string AsStringWithoutOrnaments { get; set; }
    }
}
