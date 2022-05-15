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
        private static string ExtractPatternFromPart(string asString)
        {
            var items = asString.Split(',');
            var patternLength = asString.Length;
            foreach (var i in GetDivisorsOf(items.Length))
            {
                var isPatternOfLength_i = true;
                for (var j = 0; j < items.Length / i; j++)
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
            return String.Join(",", items.Take(patternLength));
        }

        private static List<int> GetDivisorsOf(int n)
        {
            var retObj = new List<int>();
            for (var i = 2; i <= n / 2; i++)
            {
                if (n % i == 0) retObj.Add(i);
            }
            return retObj.OrderByDescending(x => x).ToList();
        }
    }
}
