using Sinphinity.Models.Pattern;
using System;

namespace SinphinityProcPatternApi.PatternExtraction
{
    public static partial class PatternsExtraction
    {
        private static bool IsPattern1PartOfPattern2(MelodyPattern pat1, MelodyPattern pat2)
        {
            // The last pair in pat1.AsString has always a deltapitch of 0. Because in pat2, there may be a note after the last note of pat1, this deltapitch would not be 0,
            // but so we don't want to include this deltapitch in the comparison. 
            // The last 2 characters of a patchAsString are always "0)" that correspond to the deltapitch of the last element
            var pat1WithoutLatPitch = pat1.AsString.Substring(0, pat1.AsString.Length - 2);
            if (pat2.AsString.Contains(pat1WithoutLatPitch)) return true;
            return false;
        }

    }
}
