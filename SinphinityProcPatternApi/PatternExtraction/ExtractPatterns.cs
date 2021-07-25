using Sinphinity.Models;
using Sinphinity.Models.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SinphinityProcPatternApi.PatternExtraction
{
    public static partial class PatternsExtraction
    {
        private static Dictionary<MelodyPattern, List<Occurrence>> ExtractPatterns(List<MelodyMatch> matches, string SongId)
        {
            var retObj = new Dictionary<MelodyPattern, List<Occurrence>>();
            foreach (var m in matches)
            {
                var patternito = new MelodyPattern(m);
                if (!retObj.Keys.Where(x => x.AreEqual(patternito)).Any())
                {
                    retObj[patternito] = new List<Occurrence>();
                }
                var patternote = retObj.Keys.Where(x => x.AreEqual(patternito)).FirstOrDefault();
                retObj[patternote] = AddPatternOccurrence(retObj[patternote], patternote, m.Slice1, SongId);
                retObj[patternote] = AddPatternOccurrence(retObj[patternote], patternote, m.Slice2, SongId);

            }
            retObj = RemovePatternsThatHappenLessThanNtimes(retObj, 4);
            retObj = RemovePatternsThatAreEqualToAnotherPatternPlusAstartingSilence(retObj);
            retObj = RemovePatternsThatOnlyHappenInsideAnotherPattern(retObj);
            return retObj;
        }
    }
}
