using Newtonsoft.Json;
using Sinphinity.Models;

namespace SinphinitySysStore.Models
{
    public class SongSimplificationEntity
    {
        public SongSimplificationEntity() { }

        public SongSimplificationEntity(SongSimplification ss, long songId) {
            NumberOfVoices = ss.NumberOfVoices;
            Notes = JsonConvert.SerializeObject(ss.Notes);
            Version = ss.Version;
            SongId = songId;
        }

        public long Id { get; set; }
        public long SongId { get; set; }
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
