using Sinphinity.Models;
using Sinphinity.Models.Pattern;
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
                tree = BuildTreeOfPatterns(voices[v], song.Bars, tree);
            }
            tree = RemovePatternsThatOccurLessThanNtimes(tree);
            tree = RemovePatternsThatHappenOnlyInsideOtherPatterns(tree);
            tree = RemovePatternsTharAreArepetitionOfaOneNotePattern(tree);
            tree = RemovePatternsIfTotalPatternsExceedsMaxPerSong(tree);

            return tree;
        }

        /// <summary>
        /// We remove patterns that consist of the repetition of the same relative note more than 3 times
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        private static Dictionary<string, HashSet<Occurrence>> RemovePatternsTharAreArepetitionOfaOneNotePattern(Dictionary<string, HashSet<Occurrence>> tree)
        {
            var stringsToRemove = new HashSet<string>();
            foreach (var pat in tree.Keys)
            {
                // check if it has more than 4 notes
                if (pat.Count(p => p == '(') <= 3) continue;
                // we find the first relative note
                var m = Regex.Match(pat, @"^[(][-]?[0-9]+,[0-9]*[)]");
                if (m.Success)
                {
                    // we remove all instances of this relative note
                    var removed = pat.Replace(m.Value, "");
                    // if there is nothing left, then the pattern is a repetition of the first note
                    if (removed.Length == 0) stringsToRemove.Add(pat);
                }
            }

            foreach (var pat in stringsToRemove) tree.Remove(pat);
            return tree;
        }

        private static Dictionary<string, HashSet<Occurrence>> RemovePatternsIfTotalPatternsExceedsMaxPerSong(Dictionary<string, HashSet<Occurrence>> tree, int maxPatternsPerSong = 50)
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


        private static Dictionary<string, HashSet<Occurrence>> RemovePatternsThatOccurLessThanNtimes(Dictionary<string, HashSet<Occurrence>> tree, int minPatternOccurrences = 4,
            int maxPatternsPerSong = 50)
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
