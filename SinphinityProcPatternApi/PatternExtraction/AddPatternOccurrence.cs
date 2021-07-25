using Sinphinity.Models;
using Sinphinity.Models.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SinphinityProcPatternApi.PatternExtraction
{
    public static partial class PatternsExtraction
    {
        private static List<Occurrence> AddPatternOccurrence(List<Occurrence> occurrences, MelodyPattern pattern, NotesSlice slice, string SongId)
        {
            var occu = new Occurrence
            {
                BarNumber = slice.BarNumber,
                Beat = slice.BeatNumberFromBarStart,
                Voice = slice.Voice,
                SongId = SongId
            };
            if (!occurrences.Where(x => x.SongId == SongId && x.BarNumber == slice.BarNumber && x.Beat == slice.BeatNumberFromBarStart && x.Voice == slice.Voice).Any())
                occurrences.Add(occu);
            return occurrences;
        }


    }
}
