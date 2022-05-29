namespace SinphinityModel.Helpers
{
    public static class StringExtensions
    {




        public static string ExpandPattern(this string asString)
        {
            if (asString.Contains('*'))
            {
                var asteriskLocation = asString.IndexOf("*");
                var pattern = asString.Substring(0, asteriskLocation);
                var repetitions = int.Parse(asString.Substring(asteriskLocation + 1, asString.Length - asteriskLocation - 1));
                var retString = pattern;
                for (var i = 1; i < repetitions; i++)
                    retString += $",{pattern}";
                return retString;

            }
            if (asString.Contains('^'))
            {
                var caretLocation = asString.IndexOf("^");
                var pattern = asString.Substring(0, caretLocation);
                var repetitions = int.Parse(asString.Substring(caretLocation + 1, asString.Length - caretLocation - 1));
                var retString = pattern;
                for (var i = 1; i < repetitions; i++)
                    retString += $",{pattern}";
                return retString.Substring(0, retString.LastIndexOf(","));

            }
            return asString;
        }



        /// <summary>
        /// There are 2 cases:
        /// 
        /// 1. The "normal" one where we have something like 1,-1,1,-1
        ///    we return 1,-1*2
        ///    
        /// 2. When we have something like 1,-1,1,-1,1
        ///    we return 1,-1^3
        ///    
        /// This is because when we look at the melody itself, we could have
        /// A,B,C,A,B,C
        /// that converted to a pitch phrase looks like 2,1,-3,2,1
        /// and we want to catalog this case as a repeating pattern
        /// 
        /// So we use the "^" symbol intead of "*" to indicate that the last value of the pattern is absent in the last repetition
        /// A,B,C,A,B,C would be expressed as 2,1,-3^2
        /// 
        /// </summary>
        /// <param name="asString"></param>
        /// <returns></returns>
        public static string ExtractPattern(this string asString)
        {
            // If patterns already extracted leave unchanged
            if (asString.Contains('*') || asString.Contains('^'))
                return asString;

            (var pattern, var repetitions) = GetPatternAndRepetitionsTypeAsterisk(asString);
            if (repetitions > 1)
                return $"{pattern}*{repetitions}";
            (pattern, repetitions) = GetPatternAndRepetitionsTypeCaret(asString);
            if (repetitions > 1)
                return $"{pattern}^{repetitions}";
            return asString;
        }

        private static (string, int) GetPatternAndRepetitionsTypeCaret(string asString)
        {
            var items = asString.Split(',');
            var patternLength = items.Length + 1;
            foreach (var i in GetDivisorsOf(items.Length + 1))
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
            if (patternLength == items.Length + 1)
                return (asString, 1);
            else
            {
                var pattern = String.Join(",", items.Take(patternLength));
                var repetitions = (items.Length + 1) / patternLength;
                if (patternLength == 1)
                    repetitions -= 1;
                return (pattern, repetitions);
            }
        }

        private static (string, int) GetPatternAndRepetitionsTypeAsterisk(string asString)
        {
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
                return (asString, 1);
            else
            {
                var pattern = String.Join(",", items.Take(patternLength));
                var repetitions = items.Length / patternLength;
                return (pattern, repetitions);
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
