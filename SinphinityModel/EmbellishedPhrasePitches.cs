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
    public class EmbellishedPhrasePitches : IPhrase
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

        /// <summary>
        /// Similar to AsString, but instead of having relative pitches, all pitches are relative to the first one.
        /// If AsString is 
        /// 2,1,-3,4,2,1
        /// AsStringAccum is
        /// 2,3,0,4,6,7
        /// </summary>
        public string AsStringAccum
        {
            get
            {
                var retObj = "";
                var sum = 0;
                for (int i = 0; i < Items.Count; i++)
                {
                    sum += Items[i];
                    if (i < Items.Count - 1)
                        retObj += $"{sum},";
                    else
                        retObj += $"{sum}";
                }
                return retObj;
            }
        }
        public string AsStringWithoutOrnaments { get; set; }
        public string AsStringWithoutOrnamentsAccum
        {
            get
            {
                var expanded = AsStringWithoutOrnaments.ExpandPattern();
                var items = expanded.Replace("+", "").Split(',').Select(x => Convert.ToInt32(x)).ToList();
                var retObj = "";
                var sum = 0;
                for (int i = 0; i < items.Count; i++)
                {
                    sum += items[i];
                    if (i < items.Count - 1)
                        retObj += $"{sum},";
                    else
                        retObj += $"{sum}";
                }
                return retObj;
            }
        }
        /// <summary>
        /// When we have something link C,D,C,E,C,F, this is equivalent to D,E,D,F,D,G (is the same pattern transposed up by 2 semitones). But they are not exactly the same
        /// according to our definition of a phrasePitches, because the first would be coded as 2,-2,4,-4,5 and the second as 2,-2,3,-3,4
        /// We use the "PhraseDistance" to compare the 2 and if it is small enough we consider the 2 phrases equivalent a we create only 1 record in the db. The AsString value is set
        /// to one of the instances, and we store all equivalent phrases in the Equivalence field. In the database, this is stored as a json object in 1 varchar column, rather than
        /// creating an extra table
        /// </summary>
        public List<string> Equivalences { get; set; }

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
                try
                {
                    expanded.Split(',').Select(x => Convert.ToInt32(x)).ToList();
                }
                catch (Exception ex)
                {

                }
                return expanded.Split(',').Select(x => Convert.ToInt32(x)).ToList();
            }
        }

        [JsonIgnore]
        public List<int> ItemsAccum
        {
            get
            {
                return AsStringAccum.Split(',').Select(x => Convert.ToInt32(x)).ToList();
            }
        }
        public Song AsSong
        {
            get
            {
                var asPhrasePitches = new PhrasePitches(AsString);
                return asPhrasePitches.AsSong;
            }
        }
    }
}
