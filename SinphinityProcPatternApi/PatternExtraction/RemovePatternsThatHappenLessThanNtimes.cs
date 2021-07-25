using Sinphinity.Models;
using Sinphinity.Models.Pattern;
using System;
using System.Collections.Generic;

namespace SinphinityProcPatternApi.PatternExtraction
{
    public static partial class PatternsExtraction
    {
        private static Dictionary<MelodyPattern, List<Occurrence>> RemovePatternsThatHappenLessThanNtimes(Dictionary<MelodyPattern, List<Occurrence>> patterns, int n)
        {
            var patternsToRemove = new List<MelodyPattern>();
            foreach (var pato in patterns.Keys)
            {
                if (patterns[pato].Count < n)
                    patternsToRemove.Add(pato);
            }
            foreach (var pato in patternsToRemove)
            {
                patterns.Remove(pato);
            }
            return patterns;
        }

    }
}
