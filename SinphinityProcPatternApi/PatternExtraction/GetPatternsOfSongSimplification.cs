using Sinphinity.Models;
using Sinphinity.Models.Pattern;
using SinphinityModel.Helpers;
using SinphinityProcPatternApi.Simplification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SinphinityProcPatternApi.PatternExtraction
{
    public static partial class PatternsExtraction
    {

        public static PatternMatrix GetPatternsOfSongSimplification(Song song,  int simplification = 1)
        {
            var songNotes = song.SongSimplifications[simplification].Notes;
            var voices = songNotes.NonPercussionVoices();

            var simplifiedNotes = SimplificationUtilities.GetSimplifiedNotes(songNotes, song.Bars);

            var matches1 = PatternsExtraction.GetMelodyMatchesWithDurationOfUpToNbeats(simplifiedNotes, song.Bars, 1);
            var matches2 = PatternsExtraction.GetMelodyMatchesWithDurationOfUpToNbeats(simplifiedNotes, song.Bars, 2);
            var matches3 = PatternsExtraction.GetMelodyMatchesWithDurationOfUpToNbeats(simplifiedNotes, song.Bars, 3);
            var matches4 = PatternsExtraction.GetMelodyMatchesWithDurationOfUpToNbeats(simplifiedNotes, song.Bars, 4);
            var patterns = ExtractPatterns(matches1.Concat(matches2).Concat(matches3).Concat(matches4).ToList(), song.Id);
            return new PatternMatrix(patterns);
        }
    }
}
