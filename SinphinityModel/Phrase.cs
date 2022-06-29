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

        public string AsString { get; set; }
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
