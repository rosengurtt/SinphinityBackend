using Sinphinity.Models;

namespace SinphinitySysStore.Models
{
    public class EmbellishedPhrasePitchesEntity
    {
        public EmbellishedPhrasePitchesEntity() { }


        public EmbellishedPhrasePitchesEntity(PhrasePitchesEntity p, string asString)
        {
            AsString = asString;
            PhrasePitchesWithoutOrnamentsId = p.Id;
        }
        /*
        public EmbellishedPhrasePitchesEntity(string originalAsString, string simplifiedAsString)
        {
            var p = new PhrasePitches(originalAsString);
            AsString = p.AsString;
            Range = p.Range;
            IsMonotone = p.IsMonotone;
            Step = p.Step;
            NumberOfNotes = p.NumberOfNotes;
            AsStringWithoutOrnaments = simplifiedAsString;
        }
        public EmbellishedPhrasePitchesEntity(EmbellishedPhrasePitchesEntity p)
        {
            Id = p.Id;
            AsString = p.AsString;
            Range = p.Range;
            IsMonotone = p.IsMonotone;
            Step = p.Step;
            NumberOfNotes = p.NumberOfNotes;
            PhrasePitchesWithoutOrnamentsId = p.PhrasePitchesWithoutOrnamentsId;
            AsStringWithoutOrnaments = p.AsStringWithoutOrnaments;
        }*/
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
        /// <summary>
        /// Link to the version of the phrase with the ornaments removed
        /// </summary>
        public long PhrasePitchesWithoutOrnamentsId { get; set; }
    }
}
