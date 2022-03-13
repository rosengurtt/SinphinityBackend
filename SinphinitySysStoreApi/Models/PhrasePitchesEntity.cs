using Sinphinity.Models;

namespace SinphinitySysStore.Models
{
    public class PhrasePitchesEntity
    {
        public PhrasePitchesEntity() { }

        public PhrasePitchesEntity(string asString)
        {
            var p = new PhrasePitches(asString);
            Id = p.Id;
            AsString = p.AsString;
            Range = p.Range;
            IsMonotone = p.IsMonotone;
            Step = p.Step;
            NumberOfNotes = p.NumberOfNotes;
        }
        public PhrasePitchesEntity(PhrasePitches p)
        {
            Id = p.Id;
            AsString = p.AsString;
            Range = p.Range;
            IsMonotone = p.IsMonotone;
            Step = p.Step;
            NumberOfNotes = p.NumberOfNotes;
        }
        /// <summary>
        /// The primary key of the record in the db
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// A comma separated list of the relative pitches
        /// </summary>
        public string AsString { get; set; }
        public int Range { get; set; }
        /// <summary>
        /// If true notes never go up or never go down
        /// </summary>
        public bool IsMonotone { get; set; }

        /// <summary>
        /// The difference between the pitch of the last and the first note
        /// </summary>
        public int Step { get; set; }
        public int NumberOfNotes { get; set; }
    }
}