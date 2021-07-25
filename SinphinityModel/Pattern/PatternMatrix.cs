using System;
using System.Collections.Generic;
using System.Linq;

namespace Sinphinity.Models.Pattern
{
    public class PatternMatrix
    {

        public PatternMatrix() { }

        /// <summary>
        /// Provides the patterns of a song and the places in the song where the patterns are used
        /// </summary>
        /// <param name="patterns"></param>
        public PatternMatrix(Dictionary<MelodyPattern, List<Occurrence>> patterns)
        {
            PatternsOfNnotes = new List<Dictionary<string, List<Occurrence>>>();
            PatternsOfNnotes.Add(null);
            PatternsOfNnotes.Add(null);
            var totalPatternsToCompute = patterns.Keys.Count;
            var patternsComputedSoFar = 0;
            var n = 2;
            while (patternsComputedSoFar < totalPatternsToCompute)
            {
                var patternsOfNnotesToAdd = patterns.Keys.Where(x => x.RelativeNotes.Count == n).ToList();
                if (patternsOfNnotesToAdd.Count > 0)
                {
                    var elementN = new Dictionary<string, List<Occurrence>>();
                    foreach (var p in patternsOfNnotesToAdd)
                        elementN[p.AsString] = patterns[p];
                    var elementNsorted = elementN.OrderBy(x => x.Value.Count).ToDictionary(x => x.Key, x => x.Value);
                    PatternsOfNnotes.Add(elementNsorted);
                    patternsComputedSoFar += patternsOfNnotesToAdd.Count;
                }
                else
                {
                    PatternsOfNnotes.Add(null);
                }
                n++;
            }
        }

        /// <summary>
        /// This is a list where the nth element of the list has the patterns with n notes
        /// 
        /// The 2 first elements of the list are always null, since we don't have patterns of 0 or 1 notes
        /// </summary>
        public List<Dictionary<string, List<Occurrence>>> PatternsOfNnotes { get; set; }
    }

}

