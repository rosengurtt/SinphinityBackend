using Sinphinity.Models;
using Sinphinity.Models.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SinphinityProcPatternApi.PatternExtraction
{
    public  static partial class PatternsExtraction
    {
        /// <summary>
        /// Gets the nth slice with a duration of m beats
        /// </summary>
        /// <param name="notes"></param>
        /// <param name="bars"></param>
        /// <param name="voice"></param>
        /// <param name="count"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        private static NotesSlice GetNsliceOfLengthMbeats(List<Note> notes, List<Bar> bars, byte voice, int count, int m)
        {
            // Find the bar number and the beat number inside the bar
            int currentBar;
            int lastBeatOfPreviousBar;
            var firstBarBeats = bars[0].TimeSignature.Numerator;
            if ((count - 1) * m < firstBarBeats)
            {
                currentBar = 1;
                lastBeatOfPreviousBar = 0;
            }
            else
            {
                currentBar = 2;
                lastBeatOfPreviousBar = firstBarBeats;
                while (lastBeatOfPreviousBar <= (count - 1) * m)
                {
                    var previousBar = currentBar - 1;
                    var previousBarBeats = bars[previousBar - 1].TimeSignature.Numerator;
                    if (lastBeatOfPreviousBar + previousBarBeats <= (count - 1) * m)
                    {
                        lastBeatOfPreviousBar += previousBarBeats;
                        currentBar++;
                    }
                    else
                        break;
                }
            }
            var beatLength = 4 * 96 / bars[currentBar - 1].TimeSignature.Denominator;
            var beat = (count - 1) * m + 1 - lastBeatOfPreviousBar;

            // currentBar and beat start in 1, so we have to substract 1
            var startTick = bars[currentBar - 1].TicksFromBeginningOfSong + (beat - 1) * beatLength;
            var endTick = startTick + beatLength * m;

            return new NotesSlice(notes, startTick, endTick, voice, bars[currentBar - 1], beat);
        }
    }
}
