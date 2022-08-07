using SinphinityModel.Helpers;
using System.Text.Json.Serialization;

namespace Sinphinity.Models
{
    /// <summary>
    /// The list of relative pitches used in a melodic phrase in the order they are played, without rythm information
    /// Relative means that is the difference in pitch between consecutive notes, not the real pitch
    /// </summary>
    public class PhrasePitches : IPhrase
    {
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


        [JsonIgnore]
        public List<int> Items
        {
            get
            {
                var expanded = AsString.ExpandPattern();
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

        public PhraseTypeEnum PhraseType
        {
            get
            {
                return PhraseTypeEnum.Pitches;
            }
        }

        public PhrasePitches() { }
        public PhrasePitches(string asString, long? id = null)
        {
            AsString = asString;
            Id = id == null ? 0 : (long)id;
            AsString = AsString.ExtractPattern();
        }
        public PhrasePitches(List<Note> notes)
        {
            if (notes.Count > 1)
            {
                for (int i = 0; i < notes.Count - 1; i++)
                {
                    AsString += (notes[i + 1].Pitch - notes[i].Pitch).ToString();
                    if (i < notes.Count - 2)
                        AsString += ",";
                }
            }
            else
                throw new Exception("Phrases of less than 2 notes not supported");
            AsString = AsString.ExtractPattern();
        }
        public Song AsSong
        {
            get
            {
                var phraseMetrics = GetPhraseMetrics();
                var phrase = new Phrase(phraseMetrics, this);

                return phrase.AsSong;
            }
        }


        private PhraseMetrics GetPhraseMetrics()
        {
            string asString = "";
            for (var i = 0; i < Items.Count; i++)
            {
                asString += "96";
                if (i < Items.Count - 1)
                    asString += ",";
            }
            return new PhraseMetrics(asString);
        }

    }
}