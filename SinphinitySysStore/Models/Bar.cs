namespace SinphinitySysStore.Models
{
    public class Bar
    {   
        public long BarNumber { get; set; }
        public long TicksFromBeginningOfSong { get; set; }

        public long EndTick
        {
            get
            {
                int standardTicksPerQuarterNote = 96;
                int barLengthInTicks = TimeSignature.Numerator * (int)standardTicksPerQuarterNote;
                return TicksFromBeginningOfSong + barLengthInTicks;
            }
        }

        public TimeSignature TimeSignature { get; set; }

        public KeySignature KeySignature { get; set; }




        /// <summary>
        /// This flag is used when quantizing the duration of notes
        /// We aproximate the durations to whole quarters, quavers, etc.
        /// and we don't want to aproximate a triplet duration by a quaver for example
        /// </summary>
        public bool HasTriplets { get; set; }


        public long TempoInMicrosecondsPerQuarterNote { get; set; }
    }
}
