using System;
using System.Collections.Generic;
using System.Linq;

namespace SinphinitySysStore.Models
{
    public class Note 
    {
        public byte Pitch { get; set; }
        public byte Volume { get; set; }
        public long StartSinceBeginningOfSongInTicks { get; set; }
        public long EndSinceBeginningOfSongInTicks { get; set; }
        public bool IsPercussion { get; set; }
        public byte Voice { get; set; }

        public byte Instrument { get; set; }

        public List<PitchBendItem> PitchBending { get; set; }
    }
}
