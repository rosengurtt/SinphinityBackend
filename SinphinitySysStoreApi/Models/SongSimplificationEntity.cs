using Newtonsoft.Json;
using Sinphinity.Models;

namespace SinphinitySysStore.Models
{
    public class SongSimplificationEntity
    {
        public SongSimplificationEntity() { }

        public SongSimplificationEntity(SongSimplification ss, Song song)
        {
            NumberOfVoices = ss.NumberOfVoices;
            Notes = JsonConvert.SerializeObject(ss.Notes);
            Version = ss.Version;
            Song = new SongEntity() { Id = song.Id };
        }

        public long Id { get; set; }
        public SongEntity Song { get; set; }
        public long Version { get; set; }
        public string Notes { get; set; }

        public long NumberOfVoices { get; set; }

        public SongSimplification AsSongSimplification()
        {
            return new SongSimplification
            {
                NumberOfVoices = this.NumberOfVoices,
                Notes = JsonConvert.DeserializeObject<List<Note>>(this.Notes),
                Version = this.Version
            };
        }
    }
}
