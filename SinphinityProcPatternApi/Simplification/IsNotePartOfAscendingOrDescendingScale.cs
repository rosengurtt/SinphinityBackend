using Sinphinity.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SinphinityProcPatternApi.Simplification
{
    static partial class SimplificationUtilities
    {
        /// <summary>
        /// Receives a group of 9 consecutive notes and checks if there are 5 consecutive notes that are all going up or all going down in pitch
        /// We also check that the duration of the note investigated is not too different from the duration of the consecutive notes
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        private static bool IsNotePartOfAscendingOrDescendingScale(List<Note> notes, Note n)
        {
            notes = notes.OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();
            var averageDuration = notes.Average(x => x.DurationInTicks);
            if (n.DurationInTicks / (double)averageDuration < 0.6) return false;

            var consecutiveAscendingNotes = 0;
            var consecutiveDescendingNotes = 0;
            // Look for ascending notes
            for (int i = 0; i < notes.Count - 1; i++)
            {
                if (notes[i].Pitch < notes[i + 1].Pitch) consecutiveAscendingNotes++;
                else
                {
                    if (consecutiveAscendingNotes >= 5) return true;
                    consecutiveAscendingNotes = 0;
                }
            }
            if (consecutiveAscendingNotes >= 5) return true;
            // Look for descending notes
            for (int i = 0; i < notes.Count - 1; i++)
            {
                if (notes[i].Pitch > notes[i + 1].Pitch) consecutiveDescendingNotes++;
                else
                {
                    if (consecutiveDescendingNotes >= 5) return true;
                    consecutiveDescendingNotes = 0;
                }
            }
            if (consecutiveDescendingNotes >= 5) return true;
            return false;
        }


    }
}
