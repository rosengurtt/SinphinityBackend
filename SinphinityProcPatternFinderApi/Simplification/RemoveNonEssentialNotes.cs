using Sinphinity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using SinphinityProcPatternFinderApi.Utilities;

namespace SinphinityProcPatternFinderApi.Simplification
{
    static partial class SimplificationUtilities
    {
        /// <summary>
        /// We remove all notes shorter than a sixteenth except in these cases:
        /// 
        /// - the note is part of an ascending or descending scale (we check monotony of at least 5 consecutive short notes that include the one we are evaluating)
        /// - there isn't any other note less than 48 ticks before or after this note
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        public static List<Note> RemoveNonEssentialNotes(List<Note> notes)
        {
            var toleranceInTicks = 4;
            var retObj = new List<Note>();
            var voices = UtilitiesCompadre.GetVoices(notes);

            foreach (var voice in voices.Keys)
            {
                var voiceNotes = voices[voice].OrderBy(n => n.StartSinceBeginningOfSongInTicks).ThenByDescending(y => y.Pitch).ToList();
                foreach (var n in voiceNotes)
                {
                    if (n.IsPercussion ||
                        n.DurationInTicks > 24 - toleranceInTicks ||
                        notes.Where(x => Math.Abs(x.StartSinceBeginningOfSongInTicks - n.StartSinceBeginningOfSongInTicks) < 48).Count() == 1)
                    {
                        retObj.Add(n);
                        continue;
                    }
                    // check if note is part of an ascending or descending scale
                    var neighboorsHigher = voiceNotes.Where(x => x.StartSinceBeginningOfSongInTicks >= n.StartSinceBeginningOfSongInTicks)
                        .OrderBy(y => y.StartSinceBeginningOfSongInTicks)
                        .Take(5);
                    var neighboorsLower = voiceNotes.Where(x => x.StartSinceBeginningOfSongInTicks < n.StartSinceBeginningOfSongInTicks)
                        .OrderByDescending(y => y.StartSinceBeginningOfSongInTicks)
                        .Take(4);
                    var neighboors = neighboorsHigher.Concat(neighboorsLower).ToList();
                    if (IsNotePartOfAscendingOrDescendingScale(neighboors, n))
                    {
                        retObj.Add(n);
                        continue;
                    }
                }
            }
            return retObj;
        }
    }
}
