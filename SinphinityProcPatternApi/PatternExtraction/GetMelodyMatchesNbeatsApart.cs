using Sinphinity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Sinphinity.Models.Pattern;
using SinphinityModel.Helpers;

namespace SinphinityProcPatternApi.PatternExtraction
{
    public  static partial class PatternsExtraction
    {
        /// <summary>
        /// Finds the slices of notes separated by n beats that match (that is, they have equal relative notes in some interval of ticks)
        /// </summary>
        /// <param name="notes"></param>
        /// <param name="bars"></param>
        /// <param name="noBeats"></param>
        /// <returns></returns>
        public static List<MelodyMatch> GetMelodyMatchesWithDurationOfUpToNbeats(List<Note> notes, List<Bar> bars, int noBeats)
        {
            var retObj = new List<MelodyMatch>();
            notes = notes.OrderBy(x => x.StartSinceBeginningOfSongInTicks).ThenByDescending(y => y.Pitch).ToList();
            var lastTick = notes.OrderByDescending(x => x.StartSinceBeginningOfSongInTicks).FirstOrDefault().StartSinceBeginningOfSongInTicks;
            var voices = notes.NonPercussionVoices();
            foreach (var v1 in voices)
            {
                foreach (var v2 in voices)
                {
                    // When v1 and v2 are different, we want to evaluate them once, having v1=0 and v2=1 will find the same patterns as v1=1 and v2=0
                    if (v2 < v1) continue;
                    var totalBeats = GetTotalBeats(bars);
                    var count1 = 1;
                    while (count1 * noBeats <= totalBeats)
                    {
                        var slice1 = GetNsliceOfLengthMbeats(notes, bars, v1, count1, noBeats);
                        if (slice1.Notes.Count < 2)
                        {
                            count1++;
                            continue;
                        }
                        var count2 = count1 + 1;
                        while (count2 * noBeats < totalBeats)
                        {
                            var slice2 = GetNsliceOfLengthMbeats(notes, bars, v2, count2, noBeats);
                            if (slice1.BarNumber==1 && slice1.BeatNumberFromBarStart==1 && slice2.BarNumber==2 && slice2.BeatNumberFromBarStart == 1)
                            {

                            }
                            var match = GetLargerMatchBetween2Slices(slice1, slice2, bars);
                            if (IsGoodMatch(match, noBeats))
                            {
                                retObj.Add(match);
                            }
                            count2++;
                        }
                        count1++;
                    }
                }
            }
            return retObj;
        }
    }
}
