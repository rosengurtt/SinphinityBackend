using Sinphinity.Models;
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
        public static Dictionary<string, HashSet<Occurrence>> BuildTreeOfPatterns(List<Note> notes, List<Bar> bars, Dictionary<string, HashSet<Occurrence>> tree,
            int minPatternLength = 2, int maxPatternLength = 15)
        {
            // the keys of the tree are successions of relative notes expressed as (deltaTick, deltaPitch) separated by semicolons
            // like (24,1);(24,-2);(24,1)
            // the values of the tree are the locations expressed as tuples (voice, ticksFromStart) where the key appears in the list of notes

            var sortedNotes = notes
                .Clone()
                .OrderBy(x => x.StartSinceBeginningOfSongInTicks).ThenByDescending(x=>x.Pitch)
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
                    // if the note starts at the same time as the previous one, ignore it
                    if (rn.StartsWith("(0,")) continue;
                    chain = rn + chain;
                    chainLength++;
                    if (chainLength < 2) continue;
                    if (!tree.ContainsKey(chain))
                        tree[chain] = new HashSet<Occurrence>();
                    var tick = sortedNotes[i - depht].StartSinceBeginningOfSongInTicks;
                    (int bar, int beat) = GetBarAndBeatOfTick(bars, tick);
                    var oc = new Occurrence
                    {
                        Voice = notes[0].Voice,
                        BarNumber = bar,
                        Beat = beat,
                        Tick = tick
                    };
                    tree[chain].Add(oc);
                }
            }
            return tree;
        }
        private static (int, int) GetBarAndBeatOfTick(List<Bar> bars, long tick)
        {
            var barNo = bars.Where(b => b.TicksFromBeginningOfSong <= tick).Count();
            var beatLength = 4 * 96 / bars[barNo - 1].TimeSignature.Denominator;
            var beat = (int)(tick - bars[barNo - 1].TicksFromBeginningOfSong) / beatLength;
            return (barNo, beat);
        }

        private static KeySignature GetKeyOfNote(List<Bar> bars, Note n)
        {
            var bar = bars.Where(x => x.TicksFromBeginningOfSong >= n.StartSinceBeginningOfSongInTicks).OrderBy(y => y.TicksFromBeginningOfSong).FirstOrDefault();
            if (bar == null)
                bar = bars.Last();
            return bar.KeySignature;
        }
    }
}
