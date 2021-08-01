using Sinphinity.Models.Pattern;
using System;
using System.Collections.Generic;
using Sinphinity.Models;
using System.Text.RegularExpressions;
using System.Linq;

namespace SinphinityProcPatternApi.PatternExtraction
{
    public static partial class PatternsExtraction
    {
        private static Dictionary<MelodyPattern, List<Occurrence>> RemovePatternsThatAreEqualToAnotherPatternPlusAstartingSilence(Dictionary<MelodyPattern, List<Occurrence>> patterns)
        {
            var patternsToRemove = new List<MelodyPattern>();
            var keys = patterns.Keys.ToList();
            for (var i = 0; i < keys.Count; i++)
            {
                for (var j = i + 1; j < keys.Count; j++)
                {
                    var pat1 = keys[i];
                    var pat2 = keys[j];
                    if (StartWithArest(pat2.AsString) && PatternAsStringWithInitialElementRemoved(pat1.AsString)== PatternAsStringWithInitialElementRemoved(pat2.AsString))
                        patternsToRemove.Add(pat1);
                }
            }
            foreach (var pat in patternsToRemove)
            {
                patterns.Remove(pat);
            }
            return patterns;
        }
        /// <summary>
        /// If a pattern written as a string starts with a silence (it has a first element that looks like (n,0) with n!=0) it removes this first element
        /// </summary>
        /// <param name="patternAsString"></param>
        /// <returns></returns>
        private static string PatternAsStringWithInitialElementRemoved(string patternAsString)
        {
            if (Regex.IsMatch(patternAsString, "^([0-9]+,0)"))
            {
                var endOfStringToRemove = patternAsString.IndexOf(")") + 1;
                return patternAsString.Substring(endOfStringToRemove, patternAsString.Length - endOfStringToRemove);
            }
            return patternAsString;
        }
        private static bool StartWithArest(string patternAsString)
        {
            return Regex.IsMatch(patternAsString, "^([0-9]+,0)");
        }
    }
}
