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
        public static Dictionary<string, List<(int, long)>> BuildTreeOfPatterns(List<Note> notes, List<Bar> bars, int maxPatternLength = 15)
        {
            // the keys of the tree are successions of relative notes expressed as (deltaTick, deltaPitch) separated by semicolons
            // like (24,1);(24,-2);(24,1)
            // the values of the tree are the locations expressed as tuples (voice, ticksFromStart) where the key appears in the list of notes
            var tree = new Dictionary<string, List<(int, long)>>();

            var sortedNotes = notes
                .Clone()
                .OrderBy(x => x.StartSinceBeginningOfSongInTicks)
                .ToList();

            for (int i = 1; i < sortedNotes.Count; i++)
            {
                var chain = "";
                for (int depht = 0; depht < maxPatternLength; depht++)
                {
                    if (i - depht < 1) break;
                    var key = GetKeyOfNote(bars, sortedNotes[i - depht]);
                    var rn = new RelativeNote(sortedNotes[i - depht], sortedNotes[i - depht - 1], 0, key).AsString;
                    chain = rn + chain;
                    if (!tree.ContainsKey(chain))
                        tree[chain] = new List<(int, long)>();
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
