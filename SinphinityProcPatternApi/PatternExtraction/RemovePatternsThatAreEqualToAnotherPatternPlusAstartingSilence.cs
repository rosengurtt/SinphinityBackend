using Sinphinity.Models.Pattern;
using System;
using System.Collections.Generic;
using Sinphinity.Models;

namespace SinphinityProcPatternApi.PatternExtraction
{
    public static partial class PatternsExtraction
    {
        private static Dictionary<MelodyPattern, List<Occurrence>> RemovePatternsThatAreEqualToAnotherPatternPlusAstartingSilence(Dictionary<MelodyPattern, List<Occurrence>> patterns)
        {
            var patternsToRemove = new List<MelodyPattern>();
            foreach (var pat1 in patterns.Keys)
            {
                foreach (var pat2 in patterns.Keys)
                {
                    if (pat1.AsString == pat2.AsString && pat1.Duration > pat2.Duration)
                        patternsToRemove.Add(pat1);
                }
            }
            foreach (var pat in patternsToRemove)
            {
                patterns.Remove(pat);
            }
            return patterns;
        }

    }
}
