using Newtonsoft.Json;
using Sinphinity.Models;

namespace SinphinitySysStore.Models
{
    public class SongEntity
    {
        public SongEntity() { }

        public SongEntity(Song song) {
            Name = song.Name;
            MidiBase64Encoded = song.MidiBase64Encoded;
            IsSongProcessed = song.IsSongProcessed;
            ArePatternsExtracted = song.ArePatternsExtracted;
            IsMidiCorrect = song.IsMidiCorrect;
            CantBeProcessed = song.CantBeProcessed;
            Band = song.Band;
            Style = song.Style;
            MidiStats = new MidiStatsEntity(song.MidiStats, song.Id);
            Bars = JsonConvert.SerializeObject(song.Bars);
            IsSongProcessed = song.IsSongProcessed;
            SongSimplifications = new List<SongSimplificationEntity>();
            foreach(var ss in song.SongSimplifications)
            {
                SongSimplifications.Add(new SongSimplificationEntity(ss, song.Id));
            }
            TempoChanges = JsonConvert.SerializeObject(song.TempoChanges);
            DurationInSeconds = song.DurationInSeconds;
            DurationInTicks = song.DurationInTicks;
            AverageTempoInBeatsPerMinute = song.AverageTempoInBeatsPerMinute;

        }

        public long Id { get; set; }
        public string Name { get; set; }
        public string MidiBase64Encoded { get; set; }
        public bool IsSongProcessed { get; set; }
        public bool ArePatternsExtracted { get; set; }
        public bool IsMidiCorrect { get; set; }

        public bool CantBeProcessed { get; set; }
        public Band Band { get; set; }
        public Style Style { get; set; }
        public MidiStatsEntity MidiStats { get; set; }

        public List<SongSimplificationEntity> SongSimplifications { get; set; }
        public string Bars { get; set; }
        public string TempoChanges { get; set; }
        public long DurationInSeconds { get; set; }
        public long DurationInTicks { get; set; }
        public long AverageTempoInBeatsPerMinute { get; set; }

        public Song AsSong()
        {
            return new Song
            {
                Name = this.Name,
                MidiBase64Encoded = this.MidiBase64Encoded,
                IsSongProcessed = this.IsSongProcessed,
                ArePatternsExtracted = this.ArePatternsExtracted,
                IsMidiCorrect = this.IsMidiCorrect,
                CantBeProcessed = this.CantBeProcessed,
                DurationInSeconds = this.DurationInSeconds,
                DurationInTicks = this.DurationInTicks,
                AverageTempoInBeatsPerMinute = this.AverageTempoInBeatsPerMinute,
                Band = this.Band,
                Style = this.Style,
                MidiStats = this.MidiStats.AsMidiStats(),
                Bars = JsonConvert.DeserializeObject<List<Bar>>(this.Bars),
                TempoChanges = JsonConvert.DeserializeObject<List<TempoChange>>(this.TempoChanges),
                SongSimplifications = this.SongSimplifications.Select(x => x.AsSongSimplification()).ToList()
            };
        }
    }
}