using Sinphinity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SinphinityProcMidi.Midi
{
    public static partial class MidiUtilities
    {
        public static int GetAverageTempoInBeatsPerMinute(List<TempoChange> tempoChanges, long songDurationInTicks)
        {
            long totalTempoAccumulated = 0;
            long intervalStart = 0;
            long tempoStart = 500000;
            foreach(var tc in tempoChanges)
            {
                var intervalInTicks = tc.TicksSinceBeginningOfSong - intervalStart;
                totalTempoAccumulated += tempoStart * intervalInTicks;
                tempoStart = tc.MicrosecondsPerQuarterNote;
                intervalStart = tc.TicksSinceBeginningOfSong;
            }
            var lastIntervalInTicks = songDurationInTicks - intervalStart;
            totalTempoAccumulated += tempoStart * lastIntervalInTicks;
            var averageTempoInMicrosecondsPerQuarterNote = totalTempoAccumulated / (double)songDurationInTicks;
            int MicrosecondsInMinute = 60000000;
            return (int)Math.Round(MicrosecondsInMinute / (double)averageTempoInMicrosecondsPerQuarterNote);
        }
    }
}
