using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SinphinityModel.Helpers
{
    public static class StringExtensions
    {
        public static string ExtractPattern(this string AsString)
        {
            if (AsString.Contains('/'))
            {
                var parts = AsString.Split('/');
                return ExtractPatternFromPart(parts[0]) + "/" + ExtractPatternFromPart(parts[1]);
            }
            return ExtractPatternFromPart(AsString);
        }

        public static string ExpandPattern(this string AsString)
        {
            if (!AsString.Contains('*'))
                return AsString;
            var asteriskLocation = AsString.IndexOf("*");
            var pattern = AsString.Substring(0, asteriskLocation);
            var repetitions = int.Parse(AsString.Substring(asteriskLocation + 1, AsString.Length - asteriskLocation - 1));
            var retString = pattern;
            for (var i = 1; i < repetitions; i++)
                retString += $",{pattern}";
            return retString;
        }
        private static string ExtractPatternFromPart(string asString)
        {
            // If patterns already extracted leave unchanged
            if (asString.Contains('*'))
                return asString;

            var items = asString.Split(',');
            var patternLength = items.Length;
            foreach (var i in GetDivisorsOf(items.Length))
            {
                var isPatternOfLength_i = true;
                for (var j = 0; j < items.Length - i; j++)
                {
                    if (items[j] != items[j + i])
                    {
                        isPatternOfLength_i = false;
                        break;
                    }
                }
                if (isPatternOfLength_i)
                {
                    patternLength = i;
                    break;
                }
            }
            if (patternLength == items.Length)
                return asString;
            else
            {
                var pattern = String.Join(",", items.Take(patternLength));
                var repetitions = items.Length / patternLength;
                return $"{pattern}*{repetitions}";
            }
        }

        private static List<int> GetDivisorsOf(int n)
        {
            var retObj = new List<int>();
            for (var i = 1; i <= n; i++)
            {
                if (n % i == 0) retObj.Add(i);
            }
            return retObj.OrderBy(x => x).ToList();
        }
    }
}
