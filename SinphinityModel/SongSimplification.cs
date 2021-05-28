using System.Collections.Generic;

namespace Sinphinity.Models
{
    public class SongSimplification
    {
        public long Version { get; set; }
        public List<Note> Notes { get; set; }

        public long NumberOfVoices { get; set; }
    }
}
