using Sinphinity.Models;
using Sinphinity.Models.Pattern;
using System;
using System.Collections.Generic;

namespace SinphinityProcPatternApi.PatternExtraction
{
    public static partial class PatternsExtraction
    {
        private static Dictionary<MelodyPattern, List<Occurrence>> RemovePatternsThatOnlyHappenInsideAnotherPattern(Dictionary<MelodyPattern, List<Occurrence>> patterns)
        {
            var i = 0;
            var patternsToRemove = new List<MelodyPattern>();
            foreach (var pat1 in patterns.Keys)
            {
                foreach (var pat2 in patterns.Keys)
                {
                    if (pat1 == pat2) continue;
                    if (IsPattern1PartOfPattern2(pat1, pat2) && patterns[pat1].Count <= patterns[pat2].Count + 3)
                    {
                        patternsToRemove.Add(pat1);
                        var soret = patterns[pat1];
                        var trolex = patterns[pat2];
                    }
                }
                i++;
            }
            foreach (var pat in patternsToRemove)
            {
                patterns.Remove(pat);
            }
            return patterns;
        }

    }
}
