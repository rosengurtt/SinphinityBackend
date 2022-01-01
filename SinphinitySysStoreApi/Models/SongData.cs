using Newtonsoft.Json;
using Sinphinity.Models;
using SinphinitySysStore.Models;

namespace SinphinitySysStore.Models
{
    public class SongData
    {
        public SongData() { }
        public SongData(Song song)
        {
            SongId = song.Id;
            MidiBase64Encoded = song.MidiBase64Encoded;
            if (song.Bars != null && song.Bars.Count > 0)
                Bars = JsonConvert.SerializeObject(song.Bars);
            if (song.SongSimplifications != null && song.SongSimplifications.Count > 0)
            {
                SongSimplifications = new List<SongSimplificationEntity>();
                foreach (var ss in song.SongSimplifications)
                {
                    SongSimplifications.Add(new SongSimplificationEntity(ss, song, this));
                }
            }
            if (song.TempoChanges != null && song.TempoChanges.Count > 0)
                TempoChanges = JsonConvert.SerializeObject(song.TempoChanges);
            if (song.Bars != null && song.Bars.Count > 0)
            {
                Bars = JsonConvert.SerializeObject(song.Bars);
            }
        }
        public long Id { get; set; }

        public long SongId { get; set; }
        public List<SongSimplificationEntity>? SongSimplifications { get; set; }

        public string? Bars { get; set; }

        public string? TempoChanges { get; set; }
        public string? MidiBase64Encoded { get; set; }
    }
}
