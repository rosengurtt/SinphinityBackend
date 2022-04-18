using Newtonsoft.Json;

namespace SinphinitySysStore.Models
{
    public class SongSimplification
    {
        public SongSimplification() { }

        public SongSimplification(Sinphinity.Models.SongSimplification ss, Sinphinity.Models.Song song, SongData songData)
        {
            NumberOfVoices = ss.NumberOfVoices;
            Notes = JsonConvert.SerializeObject(ss.Notes);
            Version = ss.Version;
            SongData = songData;
        }

        public long Id { get; set; }
        public SongData SongData { get; set; }
        public long Version { get; set; }
        public string Notes { get; set; }

        public long NumberOfVoices { get; set; }

        public Sinphinity.Models.SongSimplification AsSongSimplification()
        {
            return new Sinphinity.Models.SongSimplification
            {
                NumberOfVoices = this.NumberOfVoices,
                Notes = JsonConvert.DeserializeObject<List<Sinphinity.Models.Note>>(this.Notes),
                Version = this.Version
            };
        }
    }
}
