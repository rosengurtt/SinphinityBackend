using Sinphinity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SinphinityModel.Helpers;


namespace SinphinityProcPatternFinderApi.PatternExtraction
{
    public static partial class PatternsExtraction
    {

        public static List<RelativeNote> ConvertNotesToListOfRelativeNotes(List<Note> notes, KeySignature key)
        {
            var retObj = new List<RelativeNote>();
            var sortedNotes = notes.Clone().OrderBy(x => x.StartSinceBeginningOfSongInTicks).ThenByDescending(y => y.Pitch).ToList();

            for (var i = 1; i < sortedNotes.Count; i++)
            {
                var rn = new RelativeNote(sortedNotes[i], sortedNotes[i - 1], 0, key);
                retObj.Add(rn);
            }
            return retObj;
        }


   
    }
}
