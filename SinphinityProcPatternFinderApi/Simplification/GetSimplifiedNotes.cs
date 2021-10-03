using Sinphinity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SinphinityProcPatternFinderApi.Simplification
{
    static partial class SimplificationUtilities
    {
        public static List<Note> GetSimplifiedNotes(List<Note> notes, List<Bar> bars)
        {
            var onlyMelodicNotes = RemoveChordNotes(RemovePercussionNotes(notes));
            return RemoveNonEssentialNotes(RemoveNotesAlterations(onlyMelodicNotes, bars));
        }
    }
}
