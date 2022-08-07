using SinphinityModel.Helpers;
using System.Text.Json.Serialization;

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
    public class EmbellishedPhraseMetrics : IPhrase
    {
        public EmbellishedPhraseMetrics(string withoutEmbelishmentsAsString, string withEmbelishmentsAsString, long? id = null)
        {
            AsStringWithoutOrnaments = withoutEmbelishmentsAsString;
            AsString = withEmbelishmentsAsString;
            Id = id == null ? 0 : (long)id;
        }
        /// <summary>
        /// The primary key of the record in the db
        /// </summary>
        public long Id { get; set; }
        public string AsString { get; set; }

        /// <summary>
        /// Similar to AsString, but instead of having relative pitches, all pitches are relative to the first one.
        /// If AsString is 
        /// 24,48,24,96
        /// AsStringAccum is
        /// 24,72,96,192
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
        public long DurationInTicks
        {
            get
            {
                return Items.Sum(); ;
            }
        }
        public int NumberOfNotes
        {
            get
            {
                return Items.Count - 1;
            }
        }

        public PhraseTypeEnum PhraseType
        {
            get
            {
                return PhraseTypeEnum.EmbelishedMetrics;
            }
        }


        [JsonIgnore]
        public List<int> Items
        {
            get
            {
                var expanded = AsString.ExpandPattern();
                return expanded.Replace("+", "").Split(',').Select(x => Convert.ToInt32(x)).ToList();
            }
        }

        public Song AsSong
        {
            get
            {
                var asPhraseMetrics = new PhraseMetrics(AsString);
                return asPhraseMetrics.AsSong;
            }
        }

    }
}
