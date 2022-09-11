using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace SinphinitySysStore.Models
{
    public class Song
    {
        public Song() { }

        public Song(Sinphinity.Models.Song song)
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
            MidiBase64Encoded = song.MidiBase64Encoded;
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsSongProcessed { get; set; }
        public bool CantBeProcessed { get; set; }
        public bool ArePhrasesExtracted { get; set; }
        public bool IsMidiCorrect { get; set; }
        [NotMapped]
        public string MidiBase64Encoded { get; set; }

        public long BandId { get; set; }
        public Band Band { get; set; }
        public long StyleId { get; set; }
        public Style Style { get; set; }
        public MidiStatsEntity MidiStats { get; set; }

        public long AverageTempoInBeatsPerMinute { get; set; }

        public ICollection<Phrase> Phrases { get; set; }


        public Sinphinity.Models.Song AsSong(SongData? sd)
        {
            var songi = new Sinphinity.Models.Song
            {
                Id = this.Id,
                Name = this.Name,
                IsSongProcessed = this.IsSongProcessed,
                ArePhrasesExtracted = this.ArePhrasesExtracted,
                IsMidiCorrect = this.IsMidiCorrect,
                CantBeProcessed = this.CantBeProcessed,
                AverageTempoInBeatsPerMinute = this.AverageTempoInBeatsPerMinute,
                Band = this.Band.AsBand(),
                Style = this.Style.AsStyle(),
                MidiStats = this.MidiStats?.AsMidiStats(),
                MidiBase64Encoded=this.MidiBase64Encoded,
                SongSimplifications = new List<Sinphinity.Models.SongSimplification>()

            };
            if (sd != null)
            {
                songi.Bars = string.IsNullOrEmpty(sd.Bars) ? new List<Sinphinity.Models.Bar>() : JsonConvert.DeserializeObject<List<Sinphinity.Models.Bar>>(sd.Bars);
                songi.TempoChanges = string.IsNullOrEmpty(sd.TempoChanges) ?
                    new List<Sinphinity.Models.TempoChange>() :
                    JsonConvert.DeserializeObject<List<Sinphinity.Models.TempoChange>>(sd.TempoChanges);
                songi.MidiBase64Encoded = sd.MidiBase64Encoded;
                songi.SongSimplifications = sd.SongSimplifications?.Select(x => x.AsSongSimplification()).ToList();

            }
            return songi;
        }
    }
}