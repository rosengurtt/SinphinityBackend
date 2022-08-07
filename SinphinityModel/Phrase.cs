using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Sinphinity.Models
{
    public class Phrase: IPhrase
    {
        public long Id { get; set; }

        public PhraseMetrics PhraseMetrics { get; set; }

        public PhrasePitches PhrasePitches { get; set; }

        public Phrase() { }

        public Phrase(string asString, long? id = null)
        {
            var parts = asString.Split('/');
            PhraseMetrics = new PhraseMetrics(parts[0]);
            PhrasePitches = new PhrasePitches(parts[1]);
            Id = Id;
            AsString = asString;
        }
        public Phrase(PhraseMetrics phraseMetrics, PhrasePitches phrasePitches)
        {
            PhraseMetrics = phraseMetrics;
            PhrasePitches = phrasePitches;
            AsString = $"{phraseMetrics.AsString}/{phrasePitches.AsString}";
        }

        /// <summary>
        /// This consist of the AsString of the metrics and the AsString of the pitches separated by a forward slash like 
        /// 96,240,48,96/5,2,2,-9
        /// </summary>
        public string AsString { get; set; }
        /// <summary>
        /// Similar to AsString, but instead of having relative pitches and ticks, all pitches and ticks are relative to the first one.
        /// If AsString is 
        /// 24,48,24,96/2,1,-3,4
        /// AsStringAccum is
        /// 24,72,96,192/2,3,0,4
        /// </summary>
        public string AsStringAccum
        {
            get
            {
                if (PhraseMetrics == null || PhrasePitches == null) {
                    var parts = AsString.Split('/');
                    PhraseMetrics = new PhraseMetrics(parts[0]);
                    PhrasePitches = new PhrasePitches(parts[1]);
                }
                return $"{PhraseMetrics.AsStringAccum}/{PhrasePitches.AsStringAccum}";
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
        public long DurationInTicks { get; set; }
        public int NumberOfNotes { get; set; }
        public int Range { get; set; }
        public int Step { get; set; }
        public bool IsMonotone { get; set; }

        public Song AsSong
        {
            get
            {
                if (PhraseMetrics == null || PhrasePitches == null)
                {
                    var parts = AsString.Split("/");
                    PhraseMetrics = new PhraseMetrics(parts[0]);
                    PhrasePitches = new PhrasePitches(parts[1]);
                }

                int totalBeats = (int)Math.Ceiling((double)PhraseMetrics.DurationInTicks / 96);
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
                    Name = $"Phrase_{AsString}",
                    SongSimplifications = new List<SongSimplification>() { new SongSimplification { Notes = this.Notes, NumberOfVoices = 1, Version = 1 } },
                    Bars = new List<Bar>() { bar },
                    Band = new Band { Name = "NoBand" },
                    Style = new Style { Name = "NoStyle" },
                    MidiStats = new MidiStats { DurationInTicks = PhraseMetrics.DurationInTicks }
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

                if (PhraseMetrics == null || PhrasePitches == null)
                {
                    var parts = AsString.Split("/");
                    PhraseMetrics = new PhraseMetrics(parts[0]);
                    PhrasePitches = new PhrasePitches(parts[1]);
                }
                retObj.Add(new Note
                {
                    Pitch = startingPitch,
                    StartSinceBeginningOfSongInTicks = 0,
                    EndSinceBeginningOfSongInTicks = PhraseMetrics.Items[0],
                    Instrument = (byte)instrument,
                    Volume = 90
                });
                for (int i = 0; i < PhrasePitches.Items.Count; i++)
                {
                    currentPitch += (byte)PhrasePitches.Items[i];
                    retObj.Add(new Note
                    {
                        StartSinceBeginningOfSongInTicks = ticksFromStart + PhraseMetrics.Items[i],
                        EndSinceBeginningOfSongInTicks = i + 1 < PhrasePitches.Items.Count ?
                            ticksFromStart + PhraseMetrics.Items[i] + PhraseMetrics.Items[i + 1] :
                            ticksFromStart + PhraseMetrics.Items[i] + 96,
                        Pitch = currentPitch,
                        Instrument = (byte)instrument,
                        Volume = 90,
                        IsPercussion = false,
                        Voice = 0
                    });
                    ticksFromStart += PhraseMetrics.Items[i];
                }
                return retObj.OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();
            }
        }
        public PhraseTypeEnum PhraseType
        {
            get
            {
                return PhraseTypeEnum.Both;
            }
        }

    }
}
