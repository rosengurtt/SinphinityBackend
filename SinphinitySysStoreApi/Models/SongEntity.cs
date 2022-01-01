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
            ArePatternsExtracted = song.ArePatternsExtracted;
            IsMidiCorrect = song.IsMidiCorrect;
            CantBeProcessed = song.CantBeProcessed;
            Band = song.Band;
            Style = song.Style;
            MidiStats = new MidiStatsEntity(song.MidiStats, song);
            AverageTempoInBeatsPerMinute = song.AverageTempoInBeatsPerMinute;
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsSongProcessed { get; set; }
        public bool CantBeProcessed { get; set; }
        public bool ArePatternsExtracted { get; set; }
        public bool IsMidiCorrect { get; set; }

        //public bool CantBeProcessed { get; set; }
        public Band Band { get; set; }
        public Style Style { get; set; }
        public MidiStatsEntity MidiStats { get; set; }

        public long AverageTempoInBeatsPerMinute { get; set; }


        public Song AsSong(SongData? sd)
        {
            var songi= new Song
            {
                Id = this.Id,
                Name = this.Name,
                IsSongProcessed = this.IsSongProcessed,
                ArePatternsExtracted = this.ArePatternsExtracted,
                IsMidiCorrect = this.IsMidiCorrect,
                CantBeProcessed = this.CantBeProcessed,
                AverageTempoInBeatsPerMinute = this.AverageTempoInBeatsPerMinute,
                Band = this.Band,
                Style = this.Style,
                MidiStats = this.MidiStats?.AsMidiStats(),
                SongSimplifications= new List<SongSimplification>()

            };
            if (sd != null){
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