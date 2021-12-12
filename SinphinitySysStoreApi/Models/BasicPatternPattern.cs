namespace SinphinitySysStore.Models
{
    /// <summary>
    /// This class is used for the SQL table BasicPatternsPatterns, that is a join table between BasicPatterns and Patterns
    /// </summary>
    public class BasicPatternPattern
    {
        public long Id { get; set; }

        public long PatternId { get; set; }
        public long BasicPatternId { get; set; }


    }
}
