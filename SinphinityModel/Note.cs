using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sinphinity.Models
{
    public class Note : ICloneable
    {
        public Guid Guid { get; set; }
        public byte Pitch { get; set; }
        public byte Volume { get; set; }
        public long StartSinceBeginningOfSongInTicks { get; set; }
        public long EndSinceBeginningOfSongInTicks { get; set; }
        public bool IsPercussion { get; set; }
        public byte Voice { get; set; }
        public byte SubVoice { get; set; }

        public byte Instrument { get; set; }
       
        public int DurationInTicks
        {
            get
            {
                return (int)(EndSinceBeginningOfSongInTicks - StartSinceBeginningOfSongInTicks);
            }
        }

        public List<PitchBendItem> PitchBending { get; set; }

        public object Clone()
        {
            return new Note
            {
                Guid = this.Guid,
                EndSinceBeginningOfSongInTicks = this.EndSinceBeginningOfSongInTicks,
                StartSinceBeginningOfSongInTicks = this.StartSinceBeginningOfSongInTicks,
                Pitch = this.Pitch,
                Volume = this.Volume,
                Instrument = this.Instrument,
                PitchBending = PitchBending!=null? PitchBending.Select(s => s.Clone()).ToList():null,
                IsPercussion = this.IsPercussion,
                Voice = this.Voice,
                SubVoice = this.SubVoice
            };
        }
        public bool IsEqual(object n)
        {
            //Check for null and compare run-time types.
            if ((n == null) || !this.GetType().Equals(n.GetType()))
            {
                return false;
            }
            else
            {
                Note noty = (Note)n;
                if (noty.Pitch == Pitch &&
                    noty.Voice == Voice &&
                    noty.SubVoice == SubVoice &&
                    noty.Volume == Volume &&
                    noty.IsPercussion == IsPercussion &&
                    noty.StartSinceBeginningOfSongInTicks == StartSinceBeginningOfSongInTicks &&
                    noty.EndSinceBeginningOfSongInTicks == EndSinceBeginningOfSongInTicks &&
                    noty.Instrument == Instrument)
                    return true;
                return false;
            }
        }

    }
}
