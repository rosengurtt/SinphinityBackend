using Sinphinity.Models;

namespace SinphinitySysStore.Models
{
    public class EmbellishedPhraseMetricsEntity
    {
        public EmbellishedPhraseMetricsEntity() { }



        public EmbellishedPhraseMetricsEntity(PhraseMetricsEntity p, string asString)
        {
            AsString = asString;
            PhraseMetricsWithoutOrnamentsId = p.Id;
        }

        /// <summary>
        /// The primary key of the record in the db
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// A string representation of the phrase metrics. It consists of numbers separated by commas
        /// The numbers represent the separations between the start of the notes
        /// The last number is the duration of the last note
        /// 
        /// 2 quarters followed by 2 sixteenths would be
        /// 96,96,48,48
        /// 
        /// Accentuations can be indicated with a plus sign after a numer, like:
        /// 96,96+,48,48
        /// </summary>
        public string AsString { get; set; }
        public long DurationInTicks { get; set; }
        public int NumberOfNotes { get; set; }
        /// <summary>
        /// Link to the version of the phrase with the ornaments removed
        /// </summary>
        public long PhraseMetricsWithoutOrnamentsId { get; set; }
    }
}
