using Sinphinity.Models.Pattern;
using System;

namespace SinphinityProcPatternApi.PatternExtraction
{
    public static partial class PatternsExtraction
    {
        private static bool IsPattern1PartOfPattern2(MelodyPattern pat1, MelodyPattern pat2)
        {
            if (pat2.AsString.Contains(pat1.AsString)) return true;
            return false;
        }

    }
}
