using SinphinityModel.Helpers;
using System.Text.Json.Serialization;

namespace Sinphinity.Models
{
    /// <summary>
    /// When the original version of a phrase has embellishments we create 2 objects
    /// 
    /// - the original phrase with the embellishments (that we store in this type of object)
    /// - the simplified version of the phrase with the embellisments removed (that we store in a PhrasePitches object, that we link from here)
    /// 
    /// </summary>
    public class EmbellishedPhrasePitches
    {
        public EmbellishedPhrasePitches(string withoutEmbelishmentsAsString, string withEmbelishmentsAsString, long? id=null)
        {
            AsStringWithoutOrnaments = withoutEmbelishmentsAsString;
            AsString = withEmbelishmentsAsString;
            Id = id == null ? 0 : (long)id;
        }
        /// <summary>
        /// The primary key of the record in the db
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// A comma separated list of the relative pitches
        /// </summary>
        public string AsString { get; set; }
        public string AsStringWithoutOrnaments { get; set; }

        /// <summary>
        /// The difference between the highest absolute pitch and the lowest absolute pitch
        /// </summary>
        public int Range
        {
            get
            {
                int MaxAccum = 0;
                int MinAccum = 0;
                int currentPitch = 0;
                foreach (var item in Items)
                {
                    currentPitch += item;
                    if (currentPitch > MaxAccum)
                        MaxAccum = currentPitch;
                    if (currentPitch < MinAccum)
                        MinAccum = currentPitch;
                }
                return MaxAccum - MinAccum;
            }
        }
        /// <summary>
        /// If true notes never go up or never go down
        /// </summary>
        public bool IsMonotone
        {
            get
            {
                return Items.All(x => x >= 0) || Items.All((x => x <= 0));
            }
        }
        /// <summary>
        /// The difference between the pitch of the last and the first note
        /// </summary>
        public int Step
        {
            get
            {
                return Items.Sum();
            }
        }
        public int NumberOfNotes
        {
            get
            {
                return Items.Count + 1;
            }
        }

        public PhraseTypeEnum PhraseType
        {
            get
            {
                return PhraseTypeEnum.EmbelishedPitches;
            }
        }

        [JsonIgnore]
        public List<int> Items
        {
            get
            {
                var expanded = AsString.ExpandPattern();
                return expanded.Split(',').Select(x => Convert.ToInt32(x)).ToList();
            }
        }

    }
}
