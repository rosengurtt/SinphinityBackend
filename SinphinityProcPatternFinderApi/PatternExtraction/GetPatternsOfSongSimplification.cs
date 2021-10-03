using Sinphinity.Models;
using Sinphinity.Models.Pattern;
using SinphinityProcPatternFinderApi.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SinphinityProcPatternFinderApi.PatternExtraction
{
    public static partial class PatternsExtraction
    {

        public static PatternMatrix GetPatternsOfSongSimplification(Song song, int simplification = 1)
        {
            var voices = UtilitiesCompadre.GetVoices(song.SongSimplifications[simplification].Notes);
            foreach (var v in voices.Keys)
            {

                var tree = BuildTreeOfPatterns(voices[v], song.Bars);
            }


            return null;
        }




    }
}
