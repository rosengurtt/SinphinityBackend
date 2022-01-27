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
        /// Collects al succession of relative notes of length up to maxPatternLength present in the song
        /// </summary>
        /// <param name="notes"></param>
        /// <param name="bars"></param>
        /// <param name="patternsSet"></param>
        /// <param name="maxPatternLength"></param>
        /// <returns></returns>
        public static HashSet<string> BuildSetOfPatterns(List<Note> notes, List<Bar> bars, HashSet<string> patternsSet, int maxPatternLength = 10)
        {
            // the keys of the tree are successions of relative notes expressed as (deltaTick, deltaPitch) 
            // like (24,1)(24,-2)(24,1)
            // the values of the tree are the locations expressed as tuples (voice, ticksFromStart) where the key appears in the list of notes

            var sortedNotes = notes
                .Clone()
                .OrderBy(x => x.StartSinceBeginningOfSongInTicks).ThenByDescending(x => x.Pitch)
                .ToList();

            for (int i = 1; i < sortedNotes.Count; i++)
            {
                var chain = $"({sortedNotes[i].DurationInTicks},0)";
                var chainLength = 1;
                for (int depht = 1; depht < maxPatternLength; depht++)
                {
                    if (i - depht < 0) break;
                    var key = GetKeyOfNote(bars, sortedNotes[i - depht]);
                    var rn = new RelativeNote(sortedNotes[i - depht + 1], sortedNotes[i - depht], 0, key).AsNormalizedString;
                    // if the note starts at the same time as the previous one, ignore it
                    if (rn.StartsWith("(0,")) continue;

                    chain = rn + chain;
                    chainLength++;
                    if (chainLength >= 2)
                        patternsSet.Add(chain);
                }
            }
            return patternsSet;
        }
        //private static Dictionary<string, HashSet<Occurrence>> AddOccurrence(Dictionary<string, HashSet<Occurrence>> tree, string chain, long tick, byte voice, int bar, int beat, long songId)
        //{
        //    if (!tree.ContainsKey(chain))
        //        tree[chain] = new HashSet<Occurrence>();
        //    var oc = new Occurrence
        //    {
        //        Voice = voice,
        //        BarNumber = bar,
        //        Beat = beat,
        //        Tick = tick,
        //        SongId = songId
        //    };
        //    tree[chain].Add(oc);
        //    return tree;
        //}

        //private static (int, int) GetBarAndBeatOfTick(List<Bar> bars, long tick)
        //{
        //    var barNo = bars.Where(b => b.TicksFromBeginningOfSong <= tick).Count();
        //    var beatLength = 4 * 96 / bars[barNo - 1].TimeSignature.Denominator;
        //    var beat = (int)(tick - bars[barNo - 1].TicksFromBeginningOfSong) / beatLength;
        //    return (barNo, beat);
        //}

        private static KeySignature GetKeyOfNote(List<Bar> bars, Note n)
        {
            var bar = bars.Where(x => x.TicksFromBeginningOfSong >= n.StartSinceBeginningOfSongInTicks).OrderBy(y => y.TicksFromBeginningOfSong).FirstOrDefault();
            if (bar == null)
                bar = bars.Last();
            return bar.KeySignature;
        }
    }
}
