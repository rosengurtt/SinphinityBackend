using Sinphinity.Models;

namespace SinphinityProcMelodyAnalyser.MelodyLogic
{
    public static class SegmentsDetection
    {
        /// <summary>
        /// Given a group of consecutive notes, it looks for the places where there are changes in the segment properties
        /// For example if there are 4 consecutive notes going up, and then 5 consecutive notes going down, there is a change
        /// in zigzag index
        /// </summary>
        /// <param name="notes"></param>
        /// <param name="edgesSoFar"></param>
        /// <returns></returns>
        //public static HashSet<long> GetSegmentsEdges(List<Note> notes, HashSet<long> edgesSoFar)
        //{

        //}
        /// <summary>
        /// We look
        /// </summary>
        /// <param name="notes"></param>
        /// <param name="edgesSoFar"></param>
        /// <returns></returns>
        //private static HashSet<long> SplitAtStartOrEndOfScales(List<Note> notes, HashSet<long> edgesSoFar)
        //{
        //    var orderedEdges = edgesSoFar.ToList().OrderBy(x => x).ToList();
        //    for (var i= 0 ; i < orderedEdges.Count-1; i++)
        //    {
        //        var notesBetweenEdges = notes.Where(n=>n.StartSinceBeginningOfSongInTicks>=orderedEdges[i] && 
        //        n.StartSinceBeginningOfSongInTicks<orderedEdges[i+1]).ToList();
               
        //        if (PhraseShouldBeSplit(notesBetweenEdges)){

        //        }

        //    }

        //}

        /// <summary>
        /// It looks at a group of consecutive notes and it checks if it should be split in segments.
        /// For example if the first 10 notes are going up in a scale, and the next 6 notes go down in an arpeggio of a chord,
        /// then it should be split in the point where the scale ends and the chord starts.
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        private static bool PhraseShouldBeSplit(List<Note> notes)
        {
            var phrase = new Phrase(notes);
            // If we have few notes and the duration is not much, don't bother
            if (notes.Count <= 1 || notes.Count * phrase.DurationInTicks < 4 * 384)
                return false;
            var fullSegment = new SegmentProperties(notes);
            return !fullSegment.IsSegmentUniform();
        }
        /// <summary>
        /// Finds group of notes that do monotinically up or down
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        private static List<List<Note>> GetGroupsOfZeroZigZagIndex(List<Note> notes)
        {
            var retObj = new List<List<Note>>();

            for (var i = 0; i < notes.Count - 1; i++)
            {
                long currentStart = notes[i].StartSinceBeginningOfSongInTicks;
                var j = 1;
                var segProp = new SegmentProperties(notes.Where(x => x.StartSinceBeginningOfSongInTicks >= currentStart &&
                 x.StartSinceBeginningOfSongInTicks < notes[i + j].StartSinceBeginningOfSongInTicks).ToList());
                while (segProp.ZigzagIndex == 0)
                {
                    j++;
                    if (j + i >= notes.Count)
                        break;
                    segProp = new SegmentProperties(notes.Where(x => x.StartSinceBeginningOfSongInTicks >= currentStart &&
                 x.StartSinceBeginningOfSongInTicks < notes[i + j].StartSinceBeginningOfSongInTicks).ToList());
                }
                if (j > 2)
                {
                    retObj.Add(notes.GetRange(i, j - 1));
                    i += j - 3;
                }
            }
            return retObj;
        }


    }
}
