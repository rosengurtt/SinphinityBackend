using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sinphinity.Models
{
    /// <summary>
    /// A segment is an abstraction of a phrase. It has information about the phrase but not the exact pitches or even the
    /// relative pitches
    /// All properties are discrete (integers or longs) and in some cases we have to round some calculations to make the values discrete
    /// We want discrete values so 2 segments that have very similar values for a property, can be considered equal
    /// </summary>
    public class Segment
    {
        public Segment(Phrase p)
        {
            TotalNotes = p.MetricItems.Count;
            DurationInTicks = p.MetricItems.Sum();
            NoteDensity = (int)Math.Round(TotalNotes * 384 / (double)DurationInTicks);
            var longestNote = p.MetricItems.Max();
            var shortestNote = p.MetricItems.Min();
            MaxDurationVariation = (int)Math.Round(10 * Math.Log2(longestNote / (double)shortestNote));
            PitchDirections = GetPitchDirections(p);
            PitchStep = p.PitchItems.Sum();
            PitchRange = GetPitchRange(p);
            (AbsPitchVariation, RelPitchVariation) = GetPitchVariation(p);
            AverageInterval = (int)Math.Round(10 * p.PitchItems.Select(x => Math.Abs(x)).Average());
            Monotony = GetMonotony(p);
        }
        /// <summary>
        /// Primary key
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int TotalNotes { get; set; }

        // METRIC RELATED
        /// <summary>
        /// Duration in ticks from the start of the first note to the end of the last note
        /// </summary>
        public long DurationInTicks { get; set; }

        /// <summary>
        /// Average number of notes per 384 ticks (rounded to the nearest integer0
        /// </summary>
        public int NoteDensity { get; set; }
        /// <summary>
        /// Logarithm in base 2 of duration of longest note divided by duration of shortest note (actually the rounding of that value 
        /// multiplied by 10, so it is an integer)
        /// </summary>
        public int MaxDurationVariation { get; set; }



        // PITCH RELATED
        /// <summary>
        /// A comma separated list of "+","-" and "0" that express the sign of the relative pitches
        /// </summary>
        public string PitchDirections { get; set; }
        /// <summary>
        /// Difference in pitch between last note and first
        /// </summary>
        public int PitchStep { get; set; }
        /// <summary>
        /// The difference between the highest and the lowest pitch
        /// </summary>
        public int PitchRange { get; set; }
        /// <summary>
        /// Gives an idea of how many different pitches are played, where all Cs for ex. are considered the same pitch, regardless if
        /// they are C1 or C3, etc. 
        /// Total different pitches divided by total notes (actually the rounding of that value multiplied by 10, so it is an integer)
        /// </summary>
        public int AbsPitchVariation { get; set; }
        /// <summary>
        /// Gives an idea of how many different pitches are played, where notes separated by an octave are considered different (C3 and C4
        /// for ex. are considered different pitches)
        /// Total different pitches divided by total notes (actually the rounding of that value multiplied by 10, so it is an integer)
        /// </summary>
        public int RelPitchVariation { get; set; }
        /// <summary>
        /// Average of the absolute values of relative pitches (actually the rounding of that value multiplied by 10, so it is an integer)
        /// </summary>
        public int AverageInterval { get; set; }
        /// <summary>
        /// The number of "no-changes" in pitch direction divided by the max possible changes (actually the rounding 
        /// of that value multiplied by 10, so it is an integer)
        /// 
        /// The maximum possible value is 10 (when it is going always up or always down) and the lowest 0 (when it changes direction every time)
        /// A change of + to 0, or - to 0 counts as half change 
        /// </summary>
        public int Monotony { get; set; }

        private int GetMonotony(Phrase p)
        {
            if (TotalNotes < 3) return 0;

            var totalChanges = (new Regex(@"-,\+")).Matches(PitchDirections).Count;
            totalChanges += (new Regex(@"\+,-")).Matches(PitchDirections).Count;
            var totalHalfChanges = (new Regex(@"\+,0")).Matches(PitchDirections).Count;
            totalHalfChanges += (new Regex(@"0,\+")).Matches(PitchDirections).Count;
            totalHalfChanges += (new Regex(@"0,-")).Matches(PitchDirections).Count;
            totalHalfChanges += (new Regex(@"-,0")).Matches(PitchDirections).Count;
            var totalNoChangesTimes10 = 10 * (TotalNotes - 2) - (10 * totalChanges) - (totalHalfChanges * 5);

            return (int)Math.Round(totalNoChangesTimes10 / (double)(TotalNotes - 2));
        }


        private string GetPitchDirections(Phrase p)
        {
            var directionsAsIntegersList = p.PitchItems.Select(x => Math.Sign(x));
            var retObj = "";
            foreach (var direction in directionsAsIntegersList)
            {
                string asChar = "0";
                if (direction == 1) asChar = "+";
                if (direction == -1) asChar = "-";
                retObj += asChar + ",";
            }
            // Remove last comma
            retObj = retObj.Substring(0, retObj.Length - 1);
            return retObj;
        }

        private int GetPitchRange(Phrase p)
        {
            var pitches = p.PitchesAccumAsString.Replace("+", "").Split(",").Select(x => int.Parse(x));
            var max = Math.Max(0, pitches.Max());
            var min = Math.Min(0, pitches.Min());
            return max - min;
        }
        /// <summary>
        /// Return a tuple with the variation of absolute pitches and the variation of relative pitches
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private (int, int) GetPitchVariation(Phrase p)
        {
            // We add 128 so all pitches are positive. Otherwise we may have -2 and 10, that have the same absolute pitch
            // but their modulo 12 is different. 
            var pitches = p.PitchesAccumAsString.Replace("+", "").Split(",").Select(x => int.Parse(x) + 128);
            var sacamela = pitches.Distinct();
            var relPitchesVariation = (int)Math.Round(10 * pitches.Distinct().Count() / (double)pitches.Count());
            pitches = pitches.Select(x => x % 12);
            var absPitchesVariation = (int)Math.Round(10 * pitches.Distinct().Count() / (double)pitches.Count());
            return (absPitchesVariation, relPitchesVariation);
        }
  
    }
}
