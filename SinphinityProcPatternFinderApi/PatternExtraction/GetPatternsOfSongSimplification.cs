using Sinphinity.Models;
using SinphinityProcPatternFinderApi.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SinphinityProcPatternFinderApi.PatternExtraction
{
    public static partial class PatternsExtraction
    {
        public static HashSet<string> GetPatternsOfSongSimplification(Song song, int simplification = 1)
        {
            var voices = UtilitiesCompadre.GetVoices(song.SongSimplifications[simplification].Notes);
            var patternsSet = new HashSet<string>();
            foreach (var v in voices.Keys)
            {
                patternsSet = BuildSetOfPatterns(voices[v], song.Bars, patternsSet);
            }
            patternsSet = RemovePatternsShorterThanNticks(patternsSet);
            patternsSet = RemovePatternsThatHaveVeryLongNotes(patternsSet);
            patternsSet = RemovePatternsTharAreArepetitionOfAnotherPattern(patternsSet);

            return patternsSet;
        }
        /// <summary>
        /// Remove patterns like (2304,0)(24,0)
        /// </summary>
        /// <param name="patternsSet"></param>
        /// <returns></returns>
        public static HashSet<string> RemovePatternsThatHaveVeryLongNotes(HashSet<string> patternsSet, long maxNoteDuration = 384)
        {
            var stringsToRemove = new HashSet<string>();
            foreach (var pat in patternsSet)
            {
                var numbers = Regex.Matches(pat, @"[0-9]+");

                for (var i = 0; i < numbers.Count ; i++)
                {
                    var asNumber = int.Parse(numbers[i].Value);
                    if (asNumber>maxNoteDuration)
                        stringsToRemove.Add(pat);
                }
            }

            foreach (var pat in stringsToRemove) patternsSet.Remove(pat);
            return patternsSet;
        }
        /// <summary>
        /// We don't want to have patterns that last less than a beat
        /// </summary>
        /// <param name="patternsSet"></param>
        /// <param name="minTicks"></param>
        /// <returns></returns>
        public static HashSet<string> RemovePatternsLongerThanNbeats(HashSet<string> patternsSet, long maxBeats = 8)
        {
            var stringsToRemove = new HashSet<string>();

            foreach (var pat in patternsSet)
            {
                var asPattern = new Pattern(pat);
                if (asPattern.DurationInTicks > 96 * maxBeats)
                    stringsToRemove.Add(pat);
            }

            foreach (var pat in stringsToRemove) patternsSet.Remove(pat);
            return patternsSet;
        }

        /// <summary>
        /// We don't want to have patterns that last less than a beat
        /// </summary>
        /// <param name="patternsSet"></param>
        /// <param name="minTicks"></param>
        /// <returns></returns>
        public static HashSet<string> RemovePatternsShorterThanNticks(HashSet<string> patternsSet, long minTicks = 48)
        {
            var stringsToRemove = new HashSet<string>();
            foreach (var pat in patternsSet)
            {
                var asPattern = new Pattern(pat);
                if (asPattern.DurationInTicks < minTicks)
                    stringsToRemove.Add(pat);
            }

            foreach (var pat in stringsToRemove) patternsSet.Remove(pat);
            return patternsSet;
        }

        /// <summary>
        /// We want to remove a pattern like 
        /// (24,1)(24,-1)(24,1)(24,-1)(24,1)(24,-1)(24,1)(24,-1)
        /// but not a pattern like
        /// (24,1)(24,1)(24,1)(24,1)(24,1)(24,1)(24,1)(24,1)
        /// 
        /// The difference between the 2 is that the first is repeating exactly the same 2 notes again and again, while the second is going up the scale
        /// 
        /// So we look for a subpattern that has a step of 0, that is, it starts and ends in the same pitch
        /// </summary>
        /// <param name="patternsSet"></param>
        /// <returns></returns>
        public static HashSet<string> RemovePatternsTharAreArepetitionOfAnotherPattern(HashSet<string> patternsSet)
        {
            var stringsToRemove = new HashSet<string>();
            foreach (var pat in patternsSet)
            {
                var matches = Regex.Matches(pat, @"[(][0-9]+,[-]?[0-9]+[)]");

                var relativeNotesAsStrings = matches.Select(x => x.Value).ToList();
                for (var i = 2; i <= relativeNotesAsStrings.Count / 2; i++)
                {
                    var possibleSubPatternAsString =string.Join("", relativeNotesAsStrings.GetRange(0, i));
                    if (GetStepOfPattern(possibleSubPatternAsString) != 0) continue;
                    var m = Regex.Match(pat, @$"^({possibleSubPatternAsString.Replace(")", "[)]").Replace("(","[(]")})+$");
                    if (m.Success)
                        stringsToRemove.Add(pat);
                }      
            }

            foreach (var pat in stringsToRemove) patternsSet.Remove(pat);
            return patternsSet;
        }
        /// <summary>
        /// Returns the total difference in pitch between the end of the pattern and the beginning of the pattern. A value of 0
        /// means that the patten ends in the same note as it started
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        private static int GetStepOfPattern(string pattern)
        {
            var pitches = Regex.Matches(pattern, @",([-]?[0-9]+[)])");
            int total = 0;
            foreach (Match m in pitches)
            {
                total += int.Parse(m.Value.Replace(",", "").Replace(")", ""));
            }
            return total;
        }







    }
}
