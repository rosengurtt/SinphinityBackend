using Sinphinity.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SinphinityProcPatternApi.PatternExtraction
{
    public static partial class PatternsExtraction
    {
        private static int GetTotalBeats(List<Bar> bars)
        {
            return bars.Aggregate(0, (sum, next) => sum + next.TimeSignature.Numerator);
        }
    }
}
