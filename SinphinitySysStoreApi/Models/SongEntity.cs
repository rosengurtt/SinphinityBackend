using Newtonsoft.Json;
using Sinphinity.Models;

namespace SinphinitySysStore.Models
{
    public class SongEntity
    {
        public SongEntity() { }

        public SongEntity(Song song)
        {
            Id = song.Id;
            Name = song.Name;
            IsSongProcessed = song.IsSongProcessed;
            ArePhrasesExtracted = song.ArePhrasesExtracted;
            IsMidiCorrect = song.IsMidiCorrect;
            CantBeProcessed = song.CantBeProcessed;
            BandId = song.Band.Id;
            StyleId = song.Style.Id;
            MidiStats = new MidiStatsEntity(song.MidiStats, song);
            AverageTempoInBeatsPerMinute = song.AverageTempoInBeatsPerMinute;
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsSongProcessed { get; set; }
        public bool CantBeProcessed { get; set; }
        public bool ArePhrasesExtracted { get; set; }
        public bool IsMidiCorrect { get; set; }

        public long BandId { get; set; }
        public Band Band { get; set; }
        public long StyleId { get; set; }
        public Style Style { get; set; }
        public MidiStatsEntity MidiStats { get; set; }

        public long AverageTempoInBeatsPerMinute { get; set; }


        public Song AsSong(SongData? sd)
        {
            var songi = new Song
            {
                Id = this.Id,
                Name = this.Name,
                IsSongProcessed = this.IsSongProcessed,
                ArePhrasesExtracted = this.ArePhrasesExtracted,
                IsMidiCorrect = this.IsMidiCorrect,
                CantBeProcessed = this.CantBeProcessed,
                AverageTempoInBeatsPerMinute = this.AverageTempoInBeatsPerMinute,
                Band = this.Band,
                Style = this.Style,
                MidiStats = this.MidiStats?.AsMidiStats(),
                SongSimplifications = new List<SongSimplification>()

            };
            if (sd != null)
            {
                songi.Bars = string.IsNullOrEmpty(sd.Bars) ? new List<Bar>() : JsonConvert.DeserializeObject<List<Bar>>(sd.Bars);
                songi.TempoChanges = string.IsNullOrEmpty(sd.TempoChanges) ?
                    new List<TempoChange>() :
                    JsonConvert.DeserializeObject<List<TempoChange>>(sd.TempoChanges);
                songi.MidiBase64Encoded = sd.MidiBase64Encoded;
                songi.SongSimplifications = sd.SongSimplifications?.Select(x => x.AsSongSimplification()).ToList();

            }
            return songi;
        }
    }
}