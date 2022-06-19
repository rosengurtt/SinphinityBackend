using SinphinityModel.Helpers;
using System.Text.Json.Serialization;

namespace Sinphinity.Models
{
    /// <summary>
    /// Represents the metric of a melodic phrase
    /// </summary>
    public class PhraseMetrics : IPhrase
    {
        /// <summary>
        /// The primary key of the record in the db
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// A string representation of the phrase metrics. It consists of numbers separated by commas
        /// The numbers represent the separations between the start of the notes
        /// The first number is the duration in ticks from the beginning of the phrase to the first note
        /// The last number is the duration from the beginning of the last note to the start of the next phrase
        /// or the end of the bar if the next phrase doesnt start until more than a bar later
        /// 
        /// 2 quarters starting from 0 and followed by 2 sixteenths would be
        /// 0,96,96,48,48
        /// 
        /// Accentuations can be indicated with a plus sign after a numer, like:
        /// 96,96+,48,48
        /// </summary>
        public string AsString { get; set; }

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
                return Items.Count + 1;
            }
        }
        public PhraseTypeEnum PhraseType
        {
            get
            {
                return PhraseTypeEnum.Metrics;
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

        public PhraseMetrics() { }
        public PhraseMetrics(string asString, long? id = null, bool dontFixStrangeDurations = false)
        {
            if (!dontFixStrangeDurations)
                asString = FixStrangeDurations(asString);
            AsString = asString.ExtractPattern();
            Id = id == null ? 0 : (long)id;
        }

        /// <summary>
        /// endTick is where the phrase should end (in most cases the start of the first note after the phrase)
        /// But when there is a large gap before the next note, we end the phrase at the end of the bar
        /// 
        /// </summary>
        /// <param name="notes"></param>
        /// <param name="noteAfterPhrase"></param>
        public PhraseMetrics(List<Note> notes, bool dontFixStrangeDurations = false)
        {
            if (notes.Count < 2)
                throw new Exception("Phrases must have at least 2 notes");
            var orderedNotes = notes.OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();
            for (int i = 0; i < orderedNotes.Count - 1; i++)
            {
                AsString += (orderedNotes[i + 1].StartSinceBeginningOfSongInTicks - orderedNotes[i].StartSinceBeginningOfSongInTicks);
                if (i < orderedNotes.Count - 2)
                    AsString += ",";
            }
            if (!dontFixStrangeDurations)
                AsString = FixStrangeDurations(AsString);
            AsString = AsString.ExtractPattern();
        }
        /// <summary>
        /// Aproximates for ex. values like 94, 95, 98, 101 to 96
        /// If the previous value is an ornament, it doesn't do the aproximation
        /// </summary>
        /// <param name="asString"></param>
        /// <returns></returns>
        private static string FixStrangeDurations(string asString)
        {
            if (asString.Contains("*") || asString.Contains("^"))
                return asString;
            var durations = asString.Split(',');
            var totalNotes = durations.Length;
            var newValues = new List<int>();
            for (var i = 0; i < totalNotes; i++)
            {
                var durationInTicks = int.Parse(durations[i]);
                if (durationInTicks < 8)
                {
                    newValues.Add(durationInTicks);
                }
                else
                {
                    var embellishmentsDuration = GetEmbelishmentDurationOfNote(newValues);
                    var roundedValue = GetClosestRoundDuration(durationInTicks + embellishmentsDuration);
                    newValues.Add(roundedValue - embellishmentsDuration);
                }
            }
            return String.Join(",", newValues);
        }

        private static int GetClosestRoundDuration(int duration)
        {
            for (var j = 2; j <= 8; j++)
            {
                // Durations 12, 24, 48, 96, 192, 384, 768
                if (Math.Abs(duration - 3 * Math.Pow(2, j)) < Math.Pow(j, 1.4))
                    return (int)(3 * Math.Pow(2, j));
                // Durations 16, 32, 64, 128, 256
                if (j > 3 && Math.Abs(duration - Math.Pow(2, j)) < Math.Pow(j, 1.3) - 3)
                    return (int)(Math.Pow(2, j));
            }
            return duration;
        }
        private static int GetEmbelishmentDurationOfNote(List<int> previousDurations)
        {
            var i = 0;
            var totalDurations = previousDurations.Count;
            var embellishmentDuration = 0;
            while (i < totalDurations && previousDurations[totalDurations - i - 1] < 8)
            {
                embellishmentDuration += previousDurations[totalDurations - i - 1];
                i++;
            }
            // We consider it an embellishment if there are up to 3 short notes before a long note.
            if (i < 4)
                return embellishmentDuration;
            return 0;
        }

        public Song AsSong
        {
            get
            {
                var phrasePitches = GetPhrasePitches();
                var phrase = new Phrase(this, phrasePitches);
                return phrase.AsSong;
            }
        }



        private PhrasePitches GetPhrasePitches()
        {
            string asString = "";
            for (var i = 0; i < Items.Count; i++)
            {
                asString += "0";
                if (i < Items.Count - 1)
                    asString += ",";
            }
            return new PhrasePitches(asString);
        }
    }
}
