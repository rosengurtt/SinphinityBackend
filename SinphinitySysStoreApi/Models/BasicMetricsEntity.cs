using Sinphinity.Models;

namespace SinphinitySysStore.Models
{
    public class BasicMetricsEntity
    {
        public BasicMetricsEntity() { }

        public BasicMetricsEntity(BasicMetrics bm)
        {
            Id = bm.Id;
            AsString = bm.AsString;
            NumberOfNotes = bm.NumberOfNotes;
        }
        public long Id { get; set; }
        /// <summary>
        /// A string representation of the basic metrics. It consists of numbers separated by commas
        /// The numbers represent the separations between the start of the notes
        /// The last number is the duration of the last note
        ///  
        /// Accentuations can be indicated with a plus sign after a numer, like:
        /// 4,2+,1,1,2
        /// 
        /// </summary>
        public string AsString { get; set; }
        public int NumberOfNotes { get; set; }
    }
}
