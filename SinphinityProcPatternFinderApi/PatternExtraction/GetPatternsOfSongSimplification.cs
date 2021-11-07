using Sinphinity.Models;
using Sinphinity.Models.Pattern;
using SinphinityProcPatternFinderApi.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SinphinityProcPatternFinderApi.PatternExtraction
{
    public static partial class PatternsExtraction
    {

        public static PatternMatrix GetPatternsOfSongSimplification(Song song, int simplification = 1)
        {
            var voices = UtilitiesCompadre.GetVoices(song.SongSimplifications[simplification].Notes);
            var tree = new Dictionary<string, HashSet<(int, long)>>();
            foreach (var v in voices.Keys)
            {
                tree = BuildTreeOfPatterns(voices[v], song.Bars, tree);
            }
            tree = RemovePatternsThatOccurLessThanNtimes(tree);
            tree = RemovePatternsThatHappenOnlyInsideOtherPatterns(tree);

            return null;
        }

        private static  Dictionary<string, HashSet<(int, long)>> RemovePatternsThatOccurLessThanNtimes(Dictionary<string, HashSet<(int, long)>> tree, int minPatternOccurrences = 4)
        {

            var stringsToRemove = new List<string>();
            foreach (var pat in tree.Keys)
            {
                if (tree[pat].Count < minPatternOccurrences)
                    stringsToRemove.Add(pat);
            }
            foreach (var pat in stringsToRemove)
                tree.Remove(pat);
            return tree;
        }
        private static Dictionary<string, HashSet<(int, long)>> RemovePatternsThatHappenOnlyInsideOtherPatterns(Dictionary<string, HashSet<(int, long)>> tree, int minPatternOccurrences = 4)
        {

            var stringsToRemove = new List<string>();
            foreach (var pat1 in tree.Keys)
            {
                foreach (var pat2 in tree.Keys.Where(x => x.Length > pat1.Length))
                {
                    if (pat2.Contains(pat1) && tree[pat1].Count <= tree[pat2].Count + minPatternOccurrences)
                        stringsToRemove.Add(pat1);
                }
            }
            foreach (var pat in stringsToRemove)
                tree.Remove(pat);
            return tree;
        }

    }
}
