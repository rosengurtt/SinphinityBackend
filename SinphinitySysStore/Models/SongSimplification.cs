using System.Collections.Generic;

namespace SinphinitySysStore.Models
{
    public class SongSimplification
    {
        public long Version { get; set; }
        public List<Note> Notes { get; set; }

        public long NumberOfVoices { get; set; }
    }
}
