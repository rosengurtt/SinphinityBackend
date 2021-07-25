using Sinphinity.Models;
using Sinphinity.Models.Pattern;
using System;
using System.Linq;

namespace SinphinityProcPatternApi.PatternExtraction
{
    public static partial class PatternsExtraction
    {
        private static bool IsFirstNoteOfSlice(Note n, NotesSlice s)
        {
            var firstNote = s.Notes.OrderBy(x => x.StartSinceBeginningOfSongInTicks).FirstOrDefault();
            return n.StartSinceBeginningOfSongInTicks == firstNote.StartSinceBeginningOfSongInTicks;
        }
    }
}
