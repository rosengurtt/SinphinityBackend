using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinphinity.Models
{
    /// <summary>
    /// Basic Metric is similar to PhraseMetric but it has the "relative durations" instead of durations in ticks
    /// 
    /// So for example if we have a phrase metric of 
    /// 
    /// 48,24,48,24,24,24 
    /// 
    /// the basic metric will be
    /// 
    /// 2,1,2,1,1
    /// </summary>
    public class BasicMetrics
    {
        public long Id { get; set; }
        /// <summary>
        /// A string representation of the basic metrics. It consists of numbers separated by commas
        /// The numbers represent the separations between the start of the notes
        /// The last number is the duration of the last note
        ///  
        /// Accentuations can be indicated with a plus sign after a numer, like:
        /// 4,2+,1,1,2
        /// 
        /// </summary>
        public string AsString { get; set; }

        [NotMapped]
        public List<int> Items
        {
            get
            {
                return AsString.Replace("+", "").Split(',').Select(x => Convert.ToInt32(x)).ToList();
            }
        }


        public int NumberOfNotes
        {
            get
            {
                return Items.Count;
            }
        }

        public List<PhraseMetrics> PhraseMetrics { get; set; }

        public BasicMetrics() { }

        public BasicMetrics(string asString)
        {
            AsString = Simplify(asString);
        }
        public BasicMetrics(PhraseMetrics p)
        {
            AsString = Simplify(p.AsString);
        }
        private string Simplify(string asString)
        {
            if (asString.Contains("*") || asString.Contains("^"))
            {
                var symbol = asString.Contains("*") ? "*" : "^";
                var symbolLocation = asString.IndexOf(symbol);
                var pattern = asString.Substring(0, symbolLocation);
                var repetitions = int.Parse(asString.Substring(symbolLocation + 1, asString.Length - symbolLocation - 1));
                var theItems = pattern.Replace("+", "").Split(',').Select(x => Convert.ToInt32(x)).ToList();
                var retString = "";
                var gdc = GreatestCommonDivisor(theItems);
                retString += string.Join(',', theItems.Select(x => x / gdc));
                return $"{retString}{symbol}{repetitions}";
            }
            else
            {
                var theItems = asString.Replace("+", "").Split(',').Select(x => Convert.ToInt32(x)).ToList();
                var retString = "";
                var gdc = GreatestCommonDivisor(theItems);
                retString += string.Join(',', theItems.Select(x => x / gdc));
                return retString;
            }
        }

        private static long GreatestCommonDivisor(List<int> numbers)
        {
            var gcd = numbers[0];
            for (int i = 1; i < numbers.Count; i++)
            {
                gcd = GreatestCommonDivisor(numbers[i], gcd);
            }

            return gcd;
        }
        private static int GreatestCommonDivisor(int a, int b)
        {
            while (a != 0 && b != 0)
            {
                if (a > b)
                    a %= b;
                else
                    b %= a;
            }

            return a | b;
        }
    }
}
