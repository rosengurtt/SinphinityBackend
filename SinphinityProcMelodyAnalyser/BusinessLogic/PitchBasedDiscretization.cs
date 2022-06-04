using Sinphinity.Models;

namespace SinphinityProcMelodyAnalyser.BusinessLogic
{
    public static partial class PhraseDetection
    {
        /// <summary>
        /// When we have a group of 6 or more consecutive notes with small pitch steps between them, and suddenly we have a large pitch step
        /// we consider this a good candidate to add an edge
        /// </summary>
        /// <param name="notes"></param>
        /// <param name="bars"></param>
        /// <param name="edgesSoFar"></param>
        /// <returns></returns>
        public static HashSet<long> GetEdgesBetweenGroupsOfNotesWithSmallSteps(List<Note> notes, List<Bar> bars, HashSet<long> edgesSoFar)
        {
            var retObj = new HashSet<long>(edgesSoFar);
            var groupsOfNotesWithSmallSteps = GetGroupsOfNotesWithoutLargePitchJumps(notes, 4, 10);
            groupsOfNotesWithSmallSteps = groupsOfNotesWithSmallSteps.Union(GetGroupsOfNotesWithoutLargePitchJumps(notes, 7, 12)).ToHashSet();
            groupsOfNotesWithSmallSteps = groupsOfNotesWithSmallSteps.Union(GetGroupsOfNotesWithoutLargePitchJumps(notes, 2, 8)).ToHashSet();

            foreach (var jump in groupsOfNotesWithSmallSteps)
            {
                var edgeBefore = edgesSoFar.Where(x => x < jump).Count() > 0 ? edgesSoFar.Where(x => x < jump).Max() : jump;
                var edgeAfter = edgesSoFar.Where(x => x > jump).Count() > 0 ? edgesSoFar.Where(x => x > jump).Max() : jump;

                var lastNoteBeforeGroup = notes.Where(x => x.StartSinceBeginningOfSongInTicks < jump).OrderByDescending(x => x.StartSinceBeginningOfSongInTicks).FirstOrDefault();
                var firstNoteAfterGroup = notes.Where(x => x.StartSinceBeginningOfSongInTicks > jump).OrderBy(x => x.StartSinceBeginningOfSongInTicks).FirstOrDefault();
                if (lastNoteBeforeGroup != null && jump - edgeBefore > 192 && firstNoteAfterGroup != null && edgeAfter - jump > 192)
                {
                    retObj.Add(jump);
                }
            }
            return retObj;
        }


        /// <summary>
        /// Looks for groups of 6 or more consecutive notes that are going up (all of them) or down (all of them)
        /// Returns the tick where the group starts and where it ends (the end is the start of the first note that goes in the opposite direction or stays in the same pitch
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        private static List<(long, long)> GetGroupsOfMonotoneConsecutiveNotes(List<Note> notes, int minNotes = 6)
        {
            var retObj = new List<(long, long)>();
            if (notes == null || notes.Count < 7)
                return retObj;

            var orderedNotes = notes.OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();

            var currentStart = notes[0].StartSinceBeginningOfSongInTicks;
            var notesCount = 1;
            for (int i = 1; i < orderedNotes.Count - 1; i++)
            {
                // if notes at i-1, i and i+1 go in the same direction, increase notesCount
                if ((orderedNotes[i].Pitch - orderedNotes[i - 1].Pitch) * (orderedNotes[i + 1].Pitch - orderedNotes[i].Pitch) > 0)
                    notesCount++;
                else
                {
                    // We are at a point where the direction changes
                    if (notesCount >= minNotes)
                        retObj.Add((currentStart, orderedNotes[i].StartSinceBeginningOfSongInTicks));
                    currentStart = orderedNotes[i].StartSinceBeginningOfSongInTicks;
                    notesCount = 1;
                }
            }
            return retObj;
        }

        /// <summary>
        /// Looks for groups of consecutive notes where there are no large jumps in pitch. It returns points where there is a big jump after or before a group of at least
        /// 6 notes 
        /// </summary>
        /// <param name="notes"></param>
        /// <param name="maxJump">Is the maximum difference in pitch between consecutive notes to consider that they are close</param>
        /// <param name="minLargeJump">The minimum jump in pitch to consider this a large jump</param>
        /// <returns></returns>
        private static HashSet<long> GetGroupsOfNotesWithoutLargePitchJumps(List<Note> notes, int maxJump = 4, int minLargeJump = 10, int minNotes = 6)
        {
            var retObj = new HashSet<long>();
            if (notes == null || notes.Count < 7)
                return retObj;
            var orderedNotes = notes.OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();

            var notesCount = 1;
            for (int i = 1; i < orderedNotes.Count - 1; i++)
            {
                if (orderedNotes[i].StartSinceBeginningOfSongInTicks > 50680)
                {

                }
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

        /// <summary>
        /// When we have a group of 6 notes going in the same direction (up or down), we introduce an edge where the direction changes
        /// We don't add the edge if we would create a space bewtween edges that is less than 192 ticks
        /// </summary>
        /// <param name="notes"></param>
        /// <param name="bars"></param>
        /// <param name="edgesSoFar"></param>
        /// <returns></returns>
        public static HashSet<long> GetEdgesAtStartOrEndOfScales(List<Note> notes, List<Bar> bars, HashSet<long> edgesSoFar)
        {
            var retObj = new HashSet<long>(edgesSoFar);
            var scales = GetGroupsOfMonotoneConsecutiveNotes(notes);
            foreach ((var start, var end) in scales)
            {

                var edgeBefore = edgesSoFar.Where(x => x < start).Count() > 0 ? edgesSoFar.Where(x => x < start).Max() : start;
                var edgeAfter = edgesSoFar.Where(x => x > end).Count() > 0 ? edgesSoFar.Where(x => x > end).Min() : end;

                // Add edge before group
                var lastNoteBeforeGroup = notes.Where(x => x.StartSinceBeginningOfSongInTicks < start).OrderByDescending(x => x.StartSinceBeginningOfSongInTicks).FirstOrDefault();
                if (lastNoteBeforeGroup != null && start - edgeBefore > 192)
                    retObj.Add(start);

                // Add edge after group
                var firstNoteAfterGroup = notes.Where(x => x.StartSinceBeginningOfSongInTicks > end).OrderBy(x => x.StartSinceBeginningOfSongInTicks).FirstOrDefault();
                if (firstNoteAfterGroup != null && edgeAfter - end > 192)
                    retObj.Add(end);
            }
            return retObj;
        }
    }
}
