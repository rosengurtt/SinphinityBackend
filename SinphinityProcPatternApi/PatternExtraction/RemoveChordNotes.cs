using Sinphinity.Models;
using SinphinityModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SinphinityProcPatternApi.Simplification
{

    static partial class SimplificationUtilities
    {
        public static List<Note> RemoveChordNotes(List<Note> notes)
        {
            var retObj = notes.Clone();
            var notesToRemove = new List<Note>();
            foreach (var v in retObj.Voices())
            {
                foreach (var n in retObj.Where(x => x.Voice == v).OrderBy(y => y.StartSinceBeginningOfSongInTicks).ThenByDescending(z => z.Pitch))
                {
                    var chordNotes = retObj.Where(x => x.StartSinceBeginningOfSongInTicks == n.StartSinceBeginningOfSongInTicks &&
                    x.EndSinceBeginningOfSongInTicks == n.EndSinceBeginningOfSongInTicks && x.Pitch < n.Pitch);
                    foreach (var m in chordNotes)
                        notesToRemove.Add(m);
                }
            }
            foreach (var n in notesToRemove)
                retObj.Remove(n);
            return retObj;
        }
    }
}
