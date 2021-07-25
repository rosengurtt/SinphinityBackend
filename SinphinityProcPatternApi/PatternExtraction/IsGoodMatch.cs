using Sinphinity.Models.Pattern;

namespace SinphinityProcPatternApi.PatternExtraction
{
    public static partial class PatternsExtraction
    {
        /// <summary>
        /// Decides if the match between 2 slices is good enough to define a pattern
        /// The duration must be greater than half the number of beats
        /// The notes matching must be at least 3 
        /// The notes must have the same intervals between them, but not be exactly the same
        /// </summary>
        /// <param name="match"></param>
        /// <param name="minTicks">The minimum length the match must have</param>
        /// <returns></returns>
        private static bool IsGoodMatch(MelodyMatch match, int numberOfBeats)
        {
            if (match != null && match.DurationInBeats > numberOfBeats / (double)2 && match.Matches >= 3 &&
                match.Slice1.Notes[0].Pitch != match.Slice2.Notes[0].Pitch) return true;
            return false;
        }
    }
}
