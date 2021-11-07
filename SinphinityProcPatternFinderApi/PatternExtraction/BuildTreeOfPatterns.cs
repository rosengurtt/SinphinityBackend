using Sinphinity.Models;
using Sinphinity.Models.Pattern;
using SinphinityModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SinphinityProcPatternFinderApi.PatternExtraction
{
    public static partial class PatternsExtraction
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="notes"></param>
        /// <param name="key"></param>
        /// <param name="maxPatternLength">Is the </param>
        /// <returns></returns>
        public static Dictionary<string, HashSet<(int, long)>> BuildTreeOfPatterns(List<Note> notes, List<Bar> bars, Dictionary<string, HashSet<(int, long)>> tree,
            int minPatternLength = 2, int maxPatternLength = 15)
        {
            // the keys of the tree are successions of relative notes expressed as (deltaTick, deltaPitch) separated by semicolons
            // like (24,1);(24,-2);(24,1)
            // the values of the tree are the locations expressed as tuples (voice, ticksFromStart) where the key appears in the list of notes

            var sortedNotes = notes
                .Clone()
                .OrderBy(x => x.StartSinceBeginningOfSongInTicks)
                .ToList();

            for (int i = 1; i < sortedNotes.Count; i++)
            {
                var chain = "";
                var chainLength = 0;
                for (int depht = 1; depht < maxPatternLength; depht++)
                {
                    if (i - depht < 0) break;
                    var key = GetKeyOfNote(bars, sortedNotes[i - depht]);
                    var rn = new RelativeNote(sortedNotes[i - depht + 1], sortedNotes[i - depht], 0, key).AsString;
                    chain = rn + chain;
                    chainLength++;
                    if (chainLength < 2) continue;
                    if (!tree.ContainsKey(chain))
                        tree[chain] = new HashSet<(int, long)>();
                    tree[chain].Add((sortedNotes[i - depht].Voice, sortedNotes[i - depht].StartSinceBeginningOfSongInTicks));
                }
            }
            return tree;
        }

        private static KeySignature GetKeyOfNote(List<Bar> bars, Note n)
        {
            var bar = bars.Where(x => x.TicksFromBeginningOfSong >= n.StartSinceBeginningOfSongInTicks).OrderBy(y => y.TicksFromBeginningOfSong).FirstOrDefault();
            return bar.KeySignature;
        }
    }
}
