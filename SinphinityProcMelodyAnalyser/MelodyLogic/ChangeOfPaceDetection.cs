using Sinphinity.Models;

namespace SinphinityProcMelodyAnalyser.MelodyLogic
{
    public static class ChangeOfPaceDetection
    {

        /// <summary>
        /// When we have a group of 4 notes or more evenly spaced followed or preceeded by a note of a different duration, we consider the start and/or the end of the
        /// group a good place to insert an edge
        /// A condition to insert the edge is that we don't create a space between edges that is less than 192 ticks
        /// There is a question of where exactly we introduce the edge. If the duration of the notes of the group is shorter than the note preceding the group, then the
        /// edge is in where the first note of the group starts. But if the duration is longer, then the edge is located where the first note of the group ends
        /// Similarly when we add an edge at the end of the group
        /// </summary>
        /// <param name="notes"></param>
        /// <param name="bars"></param>
        /// <param name="edgesSoFar"></param>
        /// <returns></returns>
        public static HashSet<long> GetEdgesBetweenChangesInPacing(List<Note> notes, HashSet<long> edgesSoFar)
        {
            var retObj = new HashSet<long>(edgesSoFar);
            var groupsOfEvenlySpacedNotes = GetGroupsOfConsecutiveNotesEvenlySpaced(notes);

            foreach ((var start, var qtyOfNotes, var durationOfNotes) in groupsOfEvenlySpacedNotes)
            {
                if (qtyOfNotes < 4)
                    continue;
                var end = start + qtyOfNotes * durationOfNotes;
                var edgeBefore = edgesSoFar.Where(x => x < start).Count() > 0 ? edgesSoFar.Where(x => x < start).Max() : start;
                var edgeAfter = edgesSoFar.Where(x => x > end).Count() > 0 ? edgesSoFar.Where(x => x > end).Min() : end;

                // Add edge before group
                var lastNoteBeforeGroup = notes.Where(x => x.StartSinceBeginningOfSongInTicks < start).OrderByDescending(x => x.StartSinceBeginningOfSongInTicks).FirstOrDefault();
                if (lastNoteBeforeGroup == null || start - edgeBefore > 192)
                {
                    if (lastNoteBeforeGroup == null || durationOfNotes < lastNoteBeforeGroup.DurationInTicks)
                        retObj.Add(start);
                    else
                        retObj.Add(start + durationOfNotes);
                }

                // Add edge after group
                var firstNoteAfterGroup = notes.Where(x => x.StartSinceBeginningOfSongInTicks >= end).OrderBy(x => x.StartSinceBeginningOfSongInTicks).FirstOrDefault();
                if (firstNoteAfterGroup != null && edgeAfter - end > 192)
                {
                    if (durationOfNotes < firstNoteAfterGroup.DurationInTicks)
                        retObj.Add(end + firstNoteAfterGroup.DurationInTicks);
                    else
                        retObj.Add(end);
                }
            }
            return retObj;
        }
        /// <summary>
        /// Looks for groups of consecutive notes evenly spaced (meaning that the distance in ticks between consecutive notes is constant) and for each 
        /// returns the point where it starts, the number of consecutive notes and the duration of the note
        /// Duration here is not the actual duration of the note, but the separation in ticks from the start of the note to the start of the following note
        /// The list is ordered by the number in ticks where the group starts
        /// </summary>
        /// <param name="notes"></param>
        /// <param name=""></param>
        /// <returns></returns>
        private static List<(long, int, int)> GetGroupsOfConsecutiveNotesEvenlySpaced(List<Note> notes)
        {
            var retObj = new List<(long, int, int)>();
            if (notes.Count <= 3) return retObj;
            long currentGroupStart = notes[0].StartSinceBeginningOfSongInTicks;
            int currentGroupNoteDuration = (int)(notes[1].StartSinceBeginningOfSongInTicks - notes[0].StartSinceBeginningOfSongInTicks);
            var currentConsecutiveNotes = 1;
            for (int i = 1; i < notes.Count; i++)
            {
                var start = notes[i].StartSinceBeginningOfSongInTicks;
                var end = i < notes.Count - 1 ? notes[i + 1].StartSinceBeginningOfSongInTicks : notes[i].EndSinceBeginningOfSongInTicks;
                if (end - start == currentGroupNoteDuration)
                {
                    currentConsecutiveNotes++;
                }
                else
                {
                    if (currentConsecutiveNotes > 3)
                        retObj.Add((currentGroupStart, currentConsecutiveNotes, currentGroupNoteDuration));
                    currentConsecutiveNotes = 1;
                    currentGroupStart = start;
                    currentGroupNoteDuration = (int)(end - start);
                }
            }
            if (currentConsecutiveNotes > 1)
                retObj.Add((currentGroupStart, currentConsecutiveNotes, currentGroupNoteDuration));
            return retObj.OrderBy(x => x.Item1).ToList();
        }
    }
}
