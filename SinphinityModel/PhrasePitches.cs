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