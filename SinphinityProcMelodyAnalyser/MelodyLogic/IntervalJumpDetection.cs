using Sinphinity.Models;

namespace SinphinityProcMelodyAnalyser.MelodyLogic
{
    public class IntervalJumpDetection
    {
        /// <summary>
        /// When we have a group of 6 or more consecutive notes with small pitch steps between them, and suddenly we have a large pitch step
        /// we consider this a good candidate to add an edge
        /// </summary>
        /// <param name="notes"></param>
        /// <param name="bars"></param>
        /// <param name="edgesSoFar"></param>
        /// <returns></returns>
        public static HashSet<long> GetEdgesBetweenGroupsOfNotesWithSmallSteps(List<Note> notes, HashSet<long> edgesSoFar)
        {
            var retObj = new HashSet<long>(edgesSoFar);
            // When we split we don't want to produce phrases whose product of number of notes multiplied by duration is less than minLengthQtyNotes
            var minLengthQtyNotes = 192 * 2;
           
            var jumps = new List<(int, int)> { (2, 8), (4, 12) };
            var groupsOfNotesWithSmallSteps = new HashSet<long>();
            foreach ((var maxSizeOfSmallJump, var minSizeOfLargeJump) in jumps)
            {
                groupsOfNotesWithSmallSteps = groupsOfNotesWithSmallSteps
                    .Union(GetPointsWhereThereIsAlargeJumpBetweenNotesWithSmallJumps(notes, maxSizeOfSmallJump, minSizeOfLargeJump))
                    .ToHashSet();
            }

            foreach (var jump in groupsOfNotesWithSmallSteps)
            {
                var edgeBefore = retObj.Where(x => x < jump).Count() > 0 ? edgesSoFar.Where(x => x < jump).Max() : jump;
                var edgeAfter = retObj.Where(x => x > jump).Count() > 0 ? edgesSoFar.Where(x => x > jump).Max() : jump;

                var qtyNotesBeforeGroup = notes.Where(x => x.StartSinceBeginningOfSongInTicks < jump).Count();
                var qtyNotesAfterGroup = notes.Where(x => x.StartSinceBeginningOfSongInTicks > jump).Count();
                // Add a new edge if there are notes before the edge and the product qty notes * duration is greater than minLengthQtyNotes
                if (qtyNotesBeforeGroup > 0 && (jump - edgeBefore) * qtyNotesBeforeGroup > minLengthQtyNotes &&
                    qtyNotesAfterGroup > 0 && (edgeAfter - jump) * qtyNotesAfterGroup > minLengthQtyNotes)
                {
                    retObj.Add(jump);
                }
            }
            return retObj;
        }

        /// <summary>
        /// Looks for groups of consecutive notes where there are no large jumps in pitch. It returns points where there is a big jump after or 
        /// before a group of at least 6 notes 
        /// </summary>
        /// <param name="notes"></param>
        /// <param name="maxJump">Is the maximum difference in pitch between consecutive notes to consider that they are close</param>
        /// <param name="minLargeJump">The minimum jump in pitch to consider this a large jump</param>
        /// <returns></returns>
        private static HashSet<long> GetPointsWhereThereIsAlargeJumpBetweenNotesWithSmallJumps(List<Note> notes, int maxJump = 4, int minLargeJump = 10, int minNotes = 6)
        {
            var retObj = new HashSet<long>();
            if (notes == null || notes.Count < 7)
                return retObj;
            var orderedNotes = notes.OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();

            var notesCount = 1;
            for (int i = 1; i < orderedNotes.Count - 1; i++)
            {
                if (Math.Abs(orderedNotes[i].Pitch - orderedNotes[i - 1].Pitch) <= maxJump)
                    notesCount++;
                else
                {
                    if (notesCount >= minNotes && Math.Abs(orderedNotes[i].Pitch - orderedNotes[i - 1].Pitch) >= minLargeJump)
                        retObj.Add(orderedNotes[i].StartSinceBeginningOfSongInTicks);
                    notesCount = 1;
                }
            }
            return retObj;
        }
    }
}
