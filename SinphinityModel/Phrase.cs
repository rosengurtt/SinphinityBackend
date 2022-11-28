using Newtonsoft.Json;
using SinphinityModel.Helpers;


namespace Sinphinity.Models
{
    /// <summary>
    /// This is a representation of a sequence of notes that has some abstractions:
    /// - instead of absolute pitch values, we save relative pitches (relative to the first note)
    /// - we don't care about the duration of the notes, only the distance in ticks between the start of consecutive notes
    /// - we actually care about the duration of the last note, because that affects the duration of the phrase
    /// </summary>
    public class Phrase
    {

        public Phrase()
        {        
        }

        public Phrase(string metricsAsString, string pitchesAsString)
        {
            MetricsAsString = metricsAsString.ExtractPattern();
            PitchesAsString = pitchesAsString.ExtractPattern();
            Equivalences=new List<string>();
        }
        public Phrase(List<Note> notes)
        {
            if (notes.Count < 2)
                throw new Exception("Phrases must have at least 2 notes");
            var orderedNotes = notes.OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();
            for (int i = 0; i < orderedNotes.Count - 1; i++)
            {
                MetricsAsString += (orderedNotes[i + 1].StartSinceBeginningOfSongInTicks - orderedNotes[i].StartSinceBeginningOfSongInTicks);
                if (i < orderedNotes.Count - 2)
                    MetricsAsString += ",";
                PitchesAsString += (notes[i + 1].Pitch - notes[i].Pitch).ToString();
                if (i < notes.Count - 2)
                    PitchesAsString += ",";
            }
            MetricsAsString += "," + orderedNotes[orderedNotes.Count - 1].DurationInTicks;
            MetricsAsString = MetricsAsString.ExtractPattern();
            PitchesAsString = PitchesAsString.ExtractPattern();
            Equivalences = new List<string>();
        }
        /// <summary>
        /// Primary key
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// A string representation of the phrase metrics. It consists of numbers separated by commas
        /// The numbers represent the separations between the start of the notes
        /// The first number is the duration in ticks from the beginning of the phrase to the first note
        /// The last number is the duration of the last note
        /// 
        /// 2 quarters starting from 0 and followed by 2 sixteenths would be
        /// 0,96,96,48,48
        /// 
        /// Accentuations can be indicated with a plus sign after a number, like:
        /// 96,96+,48,48
        /// </summary>
        public string MetricsAsString { get; set; }
       /// <summary>
       /// The metrics of the phrase skeleton of this phrase
       /// </summary>
        public string SkeletonMetricsAsString { get; set; }

        [JsonIgnore]
        public List<int> MetricItems
        {
            get
            {
                var expanded = MetricsAsString.ExpandPattern();
                return expanded.Replace("+", "").Split(',').Select(x => Convert.ToInt32(x)).ToList();
            }
        }
        /// <summary>
        /// Similar to MetricsAsString, but instead of having relative pitches, all pitches are relative to the first one.
        /// If AsString is 
        /// 24,48,24,96
        /// AsStringAccum is
        /// 24,72,96,192
        /// </summary>
        public string MetricsAccumAsString
        {
            get
            {
                var retObj = "";
                var sum = 0;
                for (int i = 0; i < MetricItems.Count; i++)
                {
                    sum += MetricItems[i];
                    if (i < MetricItems.Count - 1)
                        retObj += $"{sum},";
                    else
                        retObj += $"{sum}";
                }
                return retObj;
            }
        }

        /// <summary>
        /// A comma separated list of the relative pitches
        /// </summary>
        public string PitchesAsString { get; set; }
        /// <summary>
        /// The pitches of the phrase skeleton of this phrase
        /// </summary>
        public string SkeletonPitchesAsString { get; set; }
        [JsonIgnore]
        public List<int> PitchItems
        {
            get
            {
                var expanded = PitchesAsString.ExpandPattern();
                return expanded.Split(',').Select(x => Convert.ToInt32(x)).ToList();
            }
        }
        /// <summary>
        /// Similar to AsString, but instead of having relative pitches, all pitches are relative to the first one.
        /// If AsString is 
        /// 2,1,-3,4,2,1
        /// AsStringAccum is
        /// 2,3,0,4,6,7
        /// </summary>
        public string PitchesAccumAsString
        {
            get
            {
                var retObj = "";
                var sum = 0;
                for (int i = 0; i < PitchItems.Count; i++)
                {
                    sum += PitchItems[i];
                    if (i < PitchItems.Count - 1)
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
        /// 
        /// We only care about pitches for equivalences. the metrics part must be the same in the 2 phrases
        /// </summary>
        public List<string> Equivalences { get; set; }
        public long DurationInTicks {
            get
            {
                return MetricItems.Sum(x => x);
            }
        }
        [JsonIgnore]
        public int NumberOfNotes {
            get
            {
                return PitchItems.Count + 1;
            }
        }
  


        [JsonIgnore]
        public Song AsSong
        {
            get
            {
                int totalBeats = (int)Math.Ceiling((double)DurationInTicks / 96);
                var bar = new Bar
                {
                    BarNumber = 1,
                    KeySignature = new KeySignature { key = 0, scale = Enums.ScaleType.major },
                    TimeSignature = new TimeSignature { Denominator = 4, Numerator = totalBeats },
                    TicksFromBeginningOfSong = 0
                };
                var retObj = new Song
                {
                    Id = 0,
                    Name = $"Phrase_{MetricsAsString}",
                    SongSimplifications = new List<SongSimplification>() { new SongSimplification { Notes = this.Notes, NumberOfVoices = 1, Version = 1 } },
                    Bars = new List<Bar>() { bar },
                    Band = new Band { Name = "NoBand" },
                    Style = new Style { Name = "NoStyle" },
                    MidiStats = new MidiStats { DurationInTicks = DurationInTicks }
                };

                return retObj;
            }
        }

        [JsonIgnore]
        private List<Note> Notes
        {
            get
            {
                byte startingPitch = 60;
                var instrument = 0;
                var retObj = new List<Note>();

                long ticksFromStart = 0;
                byte currentPitch = startingPitch;

            
                retObj.Add(new Note
                {
                    Pitch = startingPitch,
                    StartSinceBeginningOfSongInTicks = 0,
                    EndSinceBeginningOfSongInTicks = MetricItems[0],
                    Instrument = (byte)instrument,
                    Volume = 90
                });
                for (int i = 0; i < PitchItems.Count; i++)
                {
                    currentPitch += (byte)PitchItems[i];
                    retObj.Add(new Note
                    {
                        StartSinceBeginningOfSongInTicks = ticksFromStart + MetricItems[i],
                        EndSinceBeginningOfSongInTicks = i + 1 < PitchItems.Count ?
                            ticksFromStart + MetricItems[i] + MetricItems[i + 1] :
                            ticksFromStart + MetricItems[i] + 96,
                        Pitch = currentPitch,
                        Instrument = (byte)instrument,
                        Volume = 90,
                        IsPercussion = false,
                        Voice = 0
                    });
                    ticksFromStart += MetricItems[i];
                }
                return retObj.OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();
            }
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

    }
}
