using Sinphinity.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SinphinityProcPatternFinderApi.Simplification
{

    static partial class SimplificationUtilities
    {
        public static List<Note> RemovePercussionNotes(List<Note> notes)
        {
            return notes.Where(x => x.IsPercussion == false).ToList();
        }
    }
}
