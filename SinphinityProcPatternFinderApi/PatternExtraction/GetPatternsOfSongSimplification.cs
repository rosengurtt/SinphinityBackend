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
        public static Dictionary<string, HashSet<Occurrence>> GetPatternsOfSongSimplification(Song song, int simplification = 1)
        {
            var voices = UtilitiesCompadre.GetVoices(song.SongSimplifications[simplification].Notes);
            var tree = new Dictionary<string, HashSet<Occurrence>>();
            foreach (var v in voices.Keys)
            {
                tree = BuildTreeOfPatterns(voices[v], song.Bars, tree, song.Id);
            }
            tree = RemovePatternsShorterThanNticks(tree);
            tree = RemovePatternsThatHaveVeryLongNotes(tree);
            tree = RemovePatternsThatOccurLessThanNtimes(tree);
            tree = RemovePatternsThatHappenOnlyInsideOtherPatterns(tree);
            tree = RemovePatternsTharAreArepetitionOfAnotherPattern(tree);
            tree = RemovePatternsIfTotalPatternsExceedsMaxPerSong(tree);

            return tree;
        }
        /// <summary>
        /// Remove patterns like (2304,0)(24,0)
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        public static Dictionary<string, HashSet<Occurrence>> RemovePatternsThatHaveVeryLongNotes(Dictionary<string, HashSet<Occurrence>> tree, long maxNoteDuration = 384)
        {
            var stringsToRemove = new HashSet<string>();
            foreach (var pat in tree.Keys)
            {
                var numbers = Regex.Matches(pat, @"[0-9]+");

                for (var i = 0; i < numbers.Count ; i++)
                {
                    var asNumber = int.Parse(numbers[i].Value);
                    if (asNumber>maxNoteDuration)
                        stringsToRemove.Add(pat);
                }
            }

            foreach (var pat in stringsToRemove) tree.Remove(pat);
            return tree;
        }
        /// <summary>
        /// We don't want to have patterns that last less than a beat
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="minTicks"></param>
        /// <returns></returns>
        public static Dictionary<string, HashSet<Occurrence>> RemovePatternsShorterThanNbeats(Dictionary<string, HashSet<Occurrence>> tree, long maxBeats = 8)
        {
            var stringsToRemove = new HashSet<string>();

            foreach (var pat in tree.Keys)
            {
                var asPattern = new Pattern(pat);
                if (asPattern.DurationInTicks > 96 * maxBeats)
                    stringsToRemove.Add(pat);
            }

            foreach (var pat in stringsToRemove) tree.Remove(pat);
            return tree;
        }

        /// <summary>
        /// We don't want to have patterns that last less than a beat
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="minTicks"></param>
        /// <returns></returns>
        public static Dictionary<string, HashSet<Occurrence>> RemovePatternsShorterThanNticks(Dictionary<string, HashSet<Occurrence>> tree, long minTicks = 96)
        {
            var stringsToRemove = new HashSet<string>();
            foreach (var pat in tree.Keys)
            {
                var asPattern = new Pattern(pat);
                if (asPattern.DurationInTicks < minTicks)
                    stringsToRemove.Add(pat);
            }

            foreach (var pat in stringsToRemove) tree.Remove(pat);
            return tree;
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
        /// <param name="tree"></param>
        /// <returns></returns>
        public static Dictionary<string, HashSet<Occurrence>> RemovePatternsTharAreArepetitionOfAnotherPattern(Dictionary<string, HashSet<Occurrence>> tree)
        {
            var stringsToRemove = new HashSet<string>();
            foreach (var pat in tree.Keys)
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

            foreach (var pat in stringsToRemove) tree.Remove(pat);
            return tree;
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



        private static Dictionary<string, HashSet<Occurrence>> RemovePatternsIfTotalPatternsExceedsMaxPerSong(Dictionary<string, HashSet<Occurrence>> tree, int maxPatternsPerSong = 3000)
        {
            if (tree.Keys.Count > maxPatternsPerSong)
            {
                var quantityToRemove = tree.Keys.Count - maxPatternsPerSong;
                var stringsToRemove = new HashSet<string>();
                var lastRemovedValue = 0;
                while (stringsToRemove.Count < quantityToRemove)
                {
                    var nextValueToRemove = tree.Values.Where(x => x.Count > lastRemovedValue).Select(y => y.Count).Min();
                    foreach (var p in tree.Keys)
                    {
                        if (tree[p].Count > nextValueToRemove) continue;
                        if (stringsToRemove.Count < quantityToRemove)
                            stringsToRemove.Add(p);
                    }
                    lastRemovedValue = nextValueToRemove;
                }
                foreach (var pat in stringsToRemove) tree.Remove(pat);
            }
            return tree;
        }


        private static Dictionary<string, HashSet<Occurrence>> RemovePatternsThatOccurLessThanNtimes(Dictionary<string, HashSet<Occurrence>> tree, int minPatternOccurrences = 3,
            int maxPatternsPerSong = 100)
        {
            var stringsToRemove = new HashSet<string>();
            foreach (var pat in tree.Keys)
            {
                if (tree[pat].Count < minPatternOccurrences)
                    stringsToRemove.Add(pat);
            }
            foreach (var pat in stringsToRemove) tree.Remove(pat);

            return tree;

        }
        private static Dictionary<string, HashSet<Occurrence>> RemovePatternsThatHappenOnlyInsideOtherPatterns(Dictionary<string, HashSet<Occurrence>> tree, int maxTimes = 6)
        {

            var stringsToRemove = new HashSet<string>();
            foreach (var pat1 in tree.Keys)
            {
                foreach (var pat2 in tree.Keys.Where(x => x.Length > pat1.Length))
                {
                    if (pat2.Contains(pat1) && (tree[pat2].Count - tree[pat1].Count) <= tree[pat2].Count / maxTimes)
                        stringsToRemove.Add(pat1);
                }
            }
            foreach (var pat in stringsToRemove) tree.Remove(pat);
            return tree;
        }
    }
}
