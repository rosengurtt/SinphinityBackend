using Sinphinity.Models;
using Sinphinity.Models.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SinphinityProcPatternApi.PatternExtraction
{
    public static partial class PatternsExtraction
    {
        private static MelodyMatch GetLargerMatchBetween2Slices(NotesSlice slice1, NotesSlice slice2, List<Bar> bars)
        {
            if (slice1.Notes.Count == 0 || slice2.Notes.Count == 0) return null;

            // if the denominator of the time signatures are different we don't try to match
            if (slice1.Bar.TimeSignature.Denominator != slice2.Bar.TimeSignature.Denominator)
                return null;

            // DifferencePoints has the ticks since the beginning of the slice where there are notes that don't match
            HashSet<long> DifferencePoints = slice1.GetDifferencePoints(slice2);;


            // if slices are perfect match return the whole slices as a match
            if (DifferencePoints.Count == 0)
            {
                return new MelodyMatch
                {
                    Slice1 = slice1,
                    Slice2 = slice2,
                    Start = 0,
                    End = slice1.Duration
                };
            }
            // Find interval with the maximum number of matches


            // We have to add the start and end of the slice time, in order to have all the possible time slices to analyze
            DifferencePoints.Add(0);
            DifferencePoints.Add(slice1.Duration + 1);

            // difPoints is DifferencePoints as an ordered list
            var difPoints = DifferencePoints.ToList().OrderBy(x => x).ToList();

            // we use maxConsecutiveMatches to store the highest number of matching notes in the intervals
            var maxConsecutiveMatches = 0;
            long start = 0;
            long end = 0;
            // we iterate now on the intervals defined by the points in difPoints
            for (var i = 0; i < difPoints.Count - 1; i++)
            {
                var matchingNotes = slice1.GetRelativeNotesBetweenTicks(difPoints[i], difPoints[i + 1]);
                if (matchingNotes.Count > maxConsecutiveMatches &&
                    // The first relative note of a slice has always pitch 0, so the matching of the first notes of 2 slices, if they are the only match, are not significant
                    !(i == 0 && matchingNotes.Count == 1))
                {
                    maxConsecutiveMatches = matchingNotes.Count();
                    start = slice1.GetTickOfFirstNoteAfterTick(difPoints[i]);
                    end = difPoints[i + 1];
                }
            }
            // If lesss than2 consecutive matches return null
            if (maxConsecutiveMatches < 2) return null;

            // At this point we have an interval (start, end) (start and end are relative values) where there are maxConsecutiveMatches matches
            var notes1 = slice1
                .Notes.Where(x => x.StartSinceBeginningOfSongInTicks - slice1.StartTick >= start && x.StartSinceBeginningOfSongInTicks - slice1.StartTick < end)
                .OrderBy(y => y.StartSinceBeginningOfSongInTicks)
                .ThenByDescending(z => z.Pitch)
                .ToList();
            var notes2 = slice2.Notes.Where(x => x.StartSinceBeginningOfSongInTicks - slice2.StartTick >= start && x.StartSinceBeginningOfSongInTicks - slice2.StartTick < end)
                .OrderBy(y => y.StartSinceBeginningOfSongInTicks)
                .ThenByDescending(z => z.Pitch)
                .ToList();
            // The first note of a slice always has relative pitch 0, so it doesn't count for a match
            int countOfMatchingNotes;
            if (notes1.Count > 0)
                countOfMatchingNotes = IsFirstNoteOfSlice(notes1[0], slice1) ? notes1.Count - 1 : notes1.Count;
            else
                countOfMatchingNotes = notes1.Count;
            var (bar1, beat1) = GetBarAndBeatNumberOfTick(bars, slice1.StartTick + start);
            var (bar2, beat2) = GetBarAndBeatNumberOfTick(bars, slice2.StartTick + start);

            // We have to make this correction or otherwise the slices will extend 1 tick after the end
            if (end == slice1.Duration + 1) end--;

            return new MelodyMatch
            {
                Slice1 = new NotesSlice(notes1, slice1.StartTick + start, slice1.StartTick + end, slice1.Voice, bars[(int)(bar1 - 1)], beat1),
                Slice2 = new NotesSlice(notes2, slice2.StartTick + start, slice2.StartTick + end, slice2.Voice, bars[(int)(bar2 - 1)], beat2),
                Start = start,
                End = end > slice1.Duration ? slice1.Duration : end
            };
        }
   


    }
}
