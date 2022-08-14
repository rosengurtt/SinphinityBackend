using Sinphinity.Models;

namespace SinphinityProcMelodyAnalyser.BusinessLogic
{
    public static class PhraseDistance
    {
        /// <summary>
        /// When we have 2 phrases with the same number of notes, we define a "pitch distance" that is a measure of how similar are the 2 phrases in regards to pitches
        /// 
        /// The pitch distance is the sum of the pitch differences between the notes in the first phrase and the corresponding notes on the second phrase, divided by the number of notes
        /// 
        /// If we have for example C,E,F,D,C and D,F,G,E,D (that are equivalent phrases) their accumulated pitches will be:
        /// 4,5,2,0 and 3,5,2,0
        /// 
        /// We then compare 4 with 3, 5 with 5, 2 with 2, and 0 with 0
        /// 
        /// The distance will be 1/5
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double GetPitchDistance(Phrase p1, Phrase p2)
        {
            if (p1.NumberOfNotes != p2.NumberOfNotes)
                return int.MaxValue;
            var totalDifference = 0;

            for (var i = 0; i < p1.NumberOfNotes-1; i++)
            {
                totalDifference += Math.Abs(p1.PitchItems[i] - p2.PitchItems[i]);
            }
            return totalDifference/(double)p1.NumberOfNotes;
        }




        /// <summary>
        /// When we have 2 phrases with the same number of notes, we define a "metric distance" that is a measure of how similar are the 2 phrases in regards to timing
        /// 
        /// The distance is the sum of the differences in note durations in ticks divided by the average duration in ticks of the 2 phrases
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double GetMetricDistance(Phrase p1, Phrase p2)
        {
            if (p1.NumberOfNotes != p2.NumberOfNotes)
                return int.MaxValue;
            var totalDifference = 0;

            for (var i = 0; i < p1.NumberOfNotes - 1; i++)
            {
                totalDifference += Math.Abs(p1.MetricItems[i] - p2.MetricItems[i]);
            }
            return 2 * totalDifference / (double)(p1.DurationInTicks + p2.DurationInTicks);
        }
    }
}
