using Sinphinity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SinphinityProcPatternApi.PatternExtraction
{
    public static partial class PatternsExtraction
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bars"></param>
        /// <param name="tick"></param>
        /// <returns>Bar number, beat number inside bar</returns>
        private static (long, long) GetBarAndBeatNumberOfTick(List<Bar> bars, long tick)
        {
            var bar = bars.Where(b => b.TicksFromBeginningOfSong <= tick).OrderByDescending(y => y.TicksFromBeginningOfSong).FirstOrDefault();
            var beatLength = 4 * 96 / bar.TimeSignature.Denominator;
            return (bar.BarNumber, ((tick - bar.TicksFromBeginningOfSong) / beatLength) + 1);
        }
    }
}
