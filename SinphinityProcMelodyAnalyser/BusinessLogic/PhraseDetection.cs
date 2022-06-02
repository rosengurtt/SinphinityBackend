using Newtonsoft.Json;
using Sinphinity.Models;
using SinphinityModel.Helpers;
using SinphinityProcMelodyAnalyser.Models;
using System.Text.RegularExpressions;

namespace SinphinityProcMelodyAnalyser.BusinessLogic
{
    public static class PhraseDetection
    {

        public static PhraseInfo? GetPhraseBetweenEdges(List<Note> notes, long start, long end, long songId, byte voice, List<Bar> bars)
        {
            var phraseNotes = notes
                .Where(x => x.StartSinceBeginningOfSongInTicks >= start && x.StartSinceBeginningOfSongInTicks < end)
                .OrderBy(y => y.StartSinceBeginningOfSongInTicks)
                .ToList();

            if (phraseNotes.Count > 2)
            {
                (var hasEmbellishments, var phraseWithoutEmbellishmentNotes) = EmbelishmentsDetection.GetPhraseWithoutEmbellishments(phraseNotes);
                if (hasEmbellishments && phraseWithoutEmbellishmentNotes.Count <= 1)
                    return null;
                var metrics = hasEmbellishments ? new PhraseMetrics(phraseWithoutEmbellishmentNotes) : new PhraseMetrics(phraseNotes);
                var pitches = hasEmbellishments ? new PhrasePitches(phraseWithoutEmbellishmentNotes) : new PhrasePitches(phraseNotes);
                var embellishedMetrics = hasEmbellishments ? new PhraseMetrics(phraseNotes) : null;
                var embellishedPitches = hasEmbellishments ? new PhrasePitches(phraseNotes) : null;
                var retObj = new PhraseInfo
                {
                    Location = new SongLocation(songId, voice, start, end, bars),
                    MetricsAsString = metrics.AsString,
                    PitchesAsString = pitches.AsString,
                    EmbellishedMetricsAsString = hasEmbellishments ? embellishedMetrics.AsString : "",
                    EmbellishedPitchesAsString = hasEmbellishments ? embellishedPitches.AsString : ""
                };
                return retObj;
            }
            return null;
        }

        /// <summary>
        /// Analyzes the metric of the song to find the points where phrases start and end
        /// Returns a list with the locations represented as the number of ticks since the beginning of the song 
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        public static HashSet<long> GetPhrasesEdges(List<Note> notes, List<Bar> bars)
        {
            var edgesSoFar = new HashSet<long>();
            edgesSoFar.Add(notes[0].StartSinceBeginningOfSongInTicks);
            edgesSoFar.Add(notes[notes.Count - 1].EndSinceBeginningOfSongInTicks);

            edgesSoFar = GetEdgesOfSilencesAndLongNotes(notes, bars, edgesSoFar);
            edgesSoFar = GetEdgesBetweenChangesInPacing(notes, bars, edgesSoFar);
            edgesSoFar = GetEdgesAtStartOrEndOfScales(notes, bars, edgesSoFar);
            edgesSoFar = GetEdgesBetweenGroupsOfNotesWithSmallSteps(notes, bars, edgesSoFar);
            edgesSoFar = GetRepeatingPatterns(notes, edgesSoFar);

            // edgesSoFar = BreakPhrasesThatAreTooLong(notes, bars, edgesSoFar);
            return edgesSoFar;
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
        public static HashSet<long> GetEdgesBetweenChangesInPacing(List<Note> notes, List<Bar> bars, HashSet<long> edgesSoFar)
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
            var groupsOfNotesWithSmallSteps = GetGroupsOfNotesWithoutLargePitchJumps(notes);

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
        /// When there are 2 or more notes starting at the same time, we keep only he higher one and remove the rest
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        public static List<Note> RemoveHarmony(List<Note> notes)
        {
            var orderedNotes = notes.OrderBy(x => x.StartSinceBeginningOfSongInTicks).ThenByDescending(y => y.Pitch).ToList();
            var retObj = new List<Note>();
            retObj.Add(orderedNotes[0]);
            for (var i = 1; i < orderedNotes.Count; i++)
            {
                if (orderedNotes[i].StartSinceBeginningOfSongInTicks != orderedNotes[i - 1].StartSinceBeginningOfSongInTicks)
                    retObj.Add((Note)orderedNotes[i].Clone());
            }
            if (retObj.Where(x => x.DurationInTicks <= 0).Any()){

            }
            return retObj;
        }

        /// <summary>
        /// When we have to phrases with a metric of 27,24, 21 and another with a metric of 22,26,24, they are really the same thing and we should write them as 24,24,24
        /// This function changes the starts of the notes 
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        public static List<Note> DiscretizeTiming(List<Note> notes)
        {
            var orderedNotes = SynchronizeNotesThatShoulsStartTogether(notes);
            var retObj = new List<Note>();
            retObj.Add((Note)orderedNotes[0].Clone());

            for (var i = 1; i < orderedNotes.Count - 1; i++)
            {

                var left = orderedNotes[i - 1].StartSinceBeginningOfSongInTicks;
                var middle = orderedNotes[i].StartSinceBeginningOfSongInTicks;
                var right = orderedNotes[i + 1].StartSinceBeginningOfSongInTicks;
                var calculatedBestPoint = ClosestDiscretePoint(left, middle, right);

                var note = (Note)notes[i].Clone();
                var shift = calculatedBestPoint - note.StartSinceBeginningOfSongInTicks;
                // we change the start value if the note and its previous one are not very short notes, and if the shift is not so large that it would make the duration of the note or the previous one null or negative
                if (calculatedBestPoint < note.EndSinceBeginningOfSongInTicks && note.DurationInTicks > 6 && retObj[i - 1].DurationInTicks > Math.Max(6, -shift))
                {
                    note.StartSinceBeginningOfSongInTicks += shift;
                    if (retObj[i - 1].StartSinceBeginningOfSongInTicks < note.StartSinceBeginningOfSongInTicks)
                        retObj[i - 1].EndSinceBeginningOfSongInTicks += shift;
                }
                if (note.DurationInTicks <= 0 || retObj[i - 1].DurationInTicks <= 0 || retObj[i - 1].StartSinceBeginningOfSongInTicks > note.StartSinceBeginningOfSongInTicks)
                {

                }
                retObj.Add(note);
            }
            retObj.Add((Note)notes[notes.Count - 1].Clone());
            return retObj;
        }


        private static List<Note> SynchronizeNotesThatShoulsStartTogether(List<Note> notes)
        {
            var orderedNotes = notes.Clone().OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();

            var i = 0;
            while (i < orderedNotes.Count)
            {
                var n = orderedNotes[i];
                // We include n in notesThatShouldStartTogetherWithThis
                var notesThatShouldStartTogetherWithThis = orderedNotes
                    .Where(x => (x.DurationInTicks >= 24 && Math.Abs(x.StartSinceBeginningOfSongInTicks - n.StartSinceBeginningOfSongInTicks) < 6) ||
                    (x.DurationInTicks >= 10 && Math.Abs(x.StartSinceBeginningOfSongInTicks - n.StartSinceBeginningOfSongInTicks) < 3))
                    .ToHashSet()
                    .Union(new HashSet<Note> { n })
                    .ToList();
                if (notesThatShouldStartTogetherWithThis.Count > 1)
                {
                    var averageStart = (long)Math.Round(notesThatShouldStartTogetherWithThis.Average(x => x.StartSinceBeginningOfSongInTicks));
                    foreach (var m in notesThatShouldStartTogetherWithThis)
                    {
                        m.StartSinceBeginningOfSongInTicks = averageStart;
                    }
                }
                i += notesThatShouldStartTogetherWithThis.Count;
            }
            return orderedNotes;
        }


        private static long ClosestDiscretePoint(long left, long middle, long right)
        {
            long minDuration = Math.Min(right - middle, middle - left);
            long unit = GetUnit(minDuration);
            if (middle % unit == 0) return middle;
            long lowerValue = Math.Max( middle - middle % unit, left);
            long higherValue = Math.Min( lowerValue + unit, right);
            return (middle - lowerValue < higherValue - middle) ? lowerValue : higherValue;
        }
        private static long GetUnit(long duration)
        {
            for (var i = 1; i < 6; i++)
            {
                if (duration == 3 * Math.Pow(2, i) || duration == Math.Pow(2, i))
                    return duration;
            }
            long candidate = duration * 2 / 3;
            if (candidate < 9) return 6;
            if (candidate < 15) return 12;
            if (candidate < 19) return 16;
            if (candidate < 30) return 24;
            if (candidate < 35) return 32;
            return 48;
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
        private static List<long> GetGroupsOfNotesWithoutLargePitchJumps(List<Note> notes, int maxJump = 4, int minLargeJump = 10, int minNotes = 6)
        {
            var retObj = new List<long>();
            if (notes == null || notes.Count < 7)
                return retObj;
            var orderedNotes = notes.OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();

            var currentStart = notes[0].StartSinceBeginningOfSongInTicks;
            var notesCount = 1;
            for (int i = 1; i < orderedNotes.Count - 1; i++)
            {
                if (Math.Abs(orderedNotes[i].Pitch - orderedNotes[i - 1].Pitch) <= maxJump)
                    notesCount++;
                else
                {
                    if (notesCount >= minNotes && Math.Abs(orderedNotes[i].Pitch - orderedNotes[i - 1].Pitch) > minLargeJump)
                        retObj.Add(orderedNotes[i].StartSinceBeginningOfSongInTicks);
                    currentStart = orderedNotes[i].StartSinceBeginningOfSongInTicks;
                    notesCount = 1;
                }
            }
            return retObj;
        }

        /// <summary>
        /// When there are large spaces between consecutive starts of notes (compared with neighbooring notes) we break the melody
        /// </summary>
        /// <param name="notes"></param>
        /// <param name="bars"></param>
        /// <param name="edgesSoFar"></param>
        /// <returns></returns>
        public static HashSet<long> GetEdgesOfSilencesAndLongNotes(List<Note> notes, List<Bar> bars, HashSet<long> edgesSoFar)
        {
            var retObj = new HashSet<long>(edgesSoFar);

            var longestNotes = notes.OrderByDescending(x => x.DurationInTicks).ThenBy(y => y.StartSinceBeginningOfSongInTicks).ToList();

            for (var i = 0; i < longestNotes.Count / 2; i++)
            {
                var candidateLocation = longestNotes[i].EndSinceBeginningOfSongInTicks;
                var candidateNoteStart = longestNotes[i].StartSinceBeginningOfSongInTicks;
                if (edgesSoFar.Contains(candidateLocation))
                    continue;
                // If note is as long as a full bar or longer, then add and edge
                if (longestNotes[i].DurationInTicks >= 384)
                {
                    retObj.Add(longestNotes[i].EndSinceBeginningOfSongInTicks);
                    continue;
                }

                var candidateDuration = longestNotes[i].DurationInTicks;
                var previousEdge = edgesSoFar.Where(x => x < candidateLocation).Max();
                var followingEdge = edgesSoFar.Where(x => x > candidateLocation).Min();
                var notesBefore = notes.Where(x => x.StartSinceBeginningOfSongInTicks >= previousEdge && x.StartSinceBeginningOfSongInTicks < candidateNoteStart).Count();
                var notesAfter = notes.Where(x => x.StartSinceBeginningOfSongInTicks < followingEdge && x.StartSinceBeginningOfSongInTicks > candidateLocation).Count();
                var fourNotesBefore = notes.Where(x => x.StartSinceBeginningOfSongInTicks >= previousEdge && x.StartSinceBeginningOfSongInTicks < candidateNoteStart)
                                    .OrderByDescending(z => z.StartSinceBeginningOfSongInTicks).Take(4);
                var longestBefore = fourNotesBefore.Count() == 0 ? 0 : fourNotesBefore.Select(y => y.DurationInTicks).Max();
                var fourNotesAfter = notes.Where(x => x.StartSinceBeginningOfSongInTicks < previousEdge && x.StartSinceBeginningOfSongInTicks > candidateLocation)
                                    .OrderBy(z => z.StartSinceBeginningOfSongInTicks).Take(4);
                var longestAfter = fourNotesAfter.Count() == 0 ? 0 : fourNotesAfter.Select(y => y.DurationInTicks).Max();

                if (IsGoodCandidate(candidateLocation, candidateDuration, previousEdge, followingEdge, notesBefore, notesAfter, longestBefore, longestAfter))
                {
                    retObj.Add(candidateLocation);
                }
            }

            return retObj;
        }

        private static bool IsGoodCandidate(long candidateLocation, int candidateDuration, long previousEdge, long followingEdge, int notesBefore, 
            int notesAfter, int? longestBefore, int? longestAfter)
        {
            if (candidateLocation - previousEdge < 144 && notesBefore < 6) return false;
            if (followingEdge - candidateLocation  < 144 && notesAfter < 6) return false;
            if (longestBefore > 0.9 * candidateDuration || longestAfter > 0.9 * candidateDuration) return false;
            return true;
        }

        /// <summary>
        /// Extends the durations of notes so there are no silence gaps between consecutive notes
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        public static List<Note> GetNotesWithSilencesRemoved(List<Note> notes)
        {
            var retObj = notes.Clone();
            for (int i = 0; i < retObj.Count - 1; i++)
            {
                retObj[i].EndSinceBeginningOfSongInTicks = retObj[i + 1].StartSinceBeginningOfSongInTicks;
            }
            return retObj.OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();
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
        public static List<(long, int, int)> GetGroupsOfConsecutiveNotesEvenlySpaced(List<Note> notes)
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

        /// <summary>
        /// Looks for sections of notes between edges that consist of a pattern that repeats itself and adds edges to separate the part that consist of a repeating pattern
        /// </summary>
        /// <param name="notes"></param>
        /// <param name="edgesSoFar"></param>
        /// <returns></returns>
        public static HashSet<long> GetRepeatingPatterns(List<Note> notes, HashSet<long> edgesSoFar)
        {
            edgesSoFar = GetRepeatingPatternsOfType(notes, edgesSoFar, PhraseTypeEnum.Both);
            edgesSoFar = GetRepeatingPatternsOfType(notes, edgesSoFar, PhraseTypeEnum.Metrics);
            edgesSoFar = GetRepeatingPatternsOfType(notes, edgesSoFar, PhraseTypeEnum.Pitches);
            return GetRepeatingPatternsOfType(notes, edgesSoFar, PhraseTypeEnum.PitchDirection);
        }
        public static HashSet<long> GetRepeatingPatternsOfType(List<Note> notes, HashSet<long> edgesSoFar, PhraseTypeEnum type)
        {
            var edges = edgesSoFar.ToList().OrderBy(x => x).ToList();
            var edgesToAdd = new HashSet<long>();
            for (var i = 0; i < edges.Count - 1; i++)
            {
                var intervalNotes = notes.Where(x => x.StartSinceBeginningOfSongInTicks >= edges[i] && x.StartSinceBeginningOfSongInTicks < edges[i + 1]).ToList();
                var newEdges = GetRepeatingNotePatternSection(intervalNotes, type);
                edgesToAdd = edgesToAdd.Union(newEdges).ToHashSet();

            }
            return edgesSoFar.Union(edgesToAdd).ToHashSet();
        }


        /// <summary>
        /// Given a group of consecutive notes (or metric intervals or pitches, depending on the parameter "type"), looks for a subset of them that consist of a repeating pattern
        /// and returns the start and end of the repeating pattern section
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>

        private static HashSet<long> GetRepeatingNotePatternSection(List<Note> notes, PhraseTypeEnum type)
        {
            var retObj = new HashSet<long>();
            var orderedNotes = notes.OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();
            // if less than 8 notes leave it alone
            if (notes.Count < 8) return retObj;
            // if total duration less than 8 quarters leave it alone
            if (orderedNotes.Max(x => x.EndSinceBeginningOfSongInTicks) - orderedNotes.Min(y => y.StartSinceBeginningOfSongInTicks) < 8 * 96)
                return retObj;
            // we use this variable to avoid testing the same value twice
            var patternsAlreadyTested = new List<string>();

            // pat is a regex pattern to search for i consecutive notes
            string asString= GetNotesAsString(notes, type);
               
            
            for (var i = 1; i < notes.Count / 2; i++)
            {
                // pat is a regex pattern to search for i consecutive notes
                string pat;
                char noteSeparator;
                switch (type)
                {
                    case PhraseTypeEnum.Both:
                        pat = string.Concat(Enumerable.Repeat("[0-9]+,[-]?[0-9]+;", i));
                        noteSeparator = ';';
                        break;
                    case PhraseTypeEnum.Metrics:
                        pat = string.Concat(Enumerable.Repeat("[0-9]+,", i));
                        noteSeparator = ',';
                        break;
                    case PhraseTypeEnum.Pitches:
                        pat = string.Concat(Enumerable.Repeat("[-]?[0-9]+,", i));
                        noteSeparator = ',';
                        break;
                    case PhraseTypeEnum.PitchDirection:
                        pat = string.Concat(Enumerable.Repeat("[-+0],", i));
                        noteSeparator = ',';
                        break;
                    default:
                        throw new Exception("Que mierda me mandaron?");
                }
                foreach (Match match in Regex.Matches(asString, pat))
                {
                    // We add this test to avoid checking twice the same thing. the Regex.Matches doesn't return unique values
                    if (patternsAlreadyTested.Contains(match.Value))
                        continue;
                    patternsAlreadyTested.Add(match.Value);

                    // j is the times we repeat the pattern and then check if that repetition of patterns actually is present
                    for (var j = notes.Count / i; j > 2; j--)
                    {
                        // pat2 is the sequence of i consecutive notes repeated j times
                        // if the next "if" is true, it means the sequence "match.value" appear repeated j times
                        var pat2 = string.Concat(Enumerable.Repeat(match.Value, j));

                        if (asString.Contains(pat2))
                        {
                            var start = asString.IndexOf(pat2);
                            var notesBeforeStart = asString.Substring(0, start).Count(x => x == noteSeparator);
                            var totalNotesInRepeatedPattern = pat2.Count(x => x == noteSeparator);
                            // if we found something like x,x,x,x,x we don't want to search latter for x,x or x,x,x etc. so we add them to the patternsAlreadyTested list
                            for (var k = 2; k <= j; k++)
                                patternsAlreadyTested.Add(string.Concat(Enumerable.Repeat(match.Value, k)));

                            if (totalNotesInRepeatedPattern > 5)
                            {
                                // we add the point where the repeated pattern starts to the list of edges
                                retObj.Add(orderedNotes[notesBeforeStart].StartSinceBeginningOfSongInTicks);
                                // we add the point where the repeated pattern end to the list of edges
                                if (notesBeforeStart + totalNotesInRepeatedPattern < notes.Count)
                                    retObj.Add(orderedNotes[notesBeforeStart + totalNotesInRepeatedPattern].EndSinceBeginningOfSongInTicks);

                                // if pattern duration longer than 4 quarters, break at the beginning of each repetition
                                if (orderedNotes[notesBeforeStart + i].EndSinceBeginningOfSongInTicks - orderedNotes[notesBeforeStart].StartSinceBeginningOfSongInTicks > 4 * 96)
                                {
                                    for (var m = 1; m < j; m++)
                                    {
                                        retObj.Add(orderedNotes[notesBeforeStart + m * i].StartSinceBeginningOfSongInTicks);
                                    }
                                }
                            }
                            // if we found j consecutive matches, there is no point to keep trying with smaller values of j, so break and continue with the next i
                            break;
                        }
                    }
                }
            }
            return retObj;
        }





        /// <summary>
        /// Creates strings like 48,3;24,-1;24,2
        /// that represent consecutive relative notes, using a comma to separate duration from pitch and semicolon between consecutive notes
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        private static string GetNotesAsString(List<Note> notes, PhraseTypeEnum type)
        {
            var orderedNotes = notes.OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();
                    var asString = "";
            switch (type)
            {
                case PhraseTypeEnum.Both:
                    for (var i = 0; i < orderedNotes.Count - 1; i++)
                    {
                        asString += $"{orderedNotes[i + 1].StartSinceBeginningOfSongInTicks - orderedNotes[i].StartSinceBeginningOfSongInTicks},{orderedNotes[i + 1].Pitch - orderedNotes[i].Pitch};";
                    }
                    return asString;
                case PhraseTypeEnum.Metrics:
                    return string.Join(",", orderedNotes.Select(x => x.DurationInTicks.ToString())) + ",";
                case PhraseTypeEnum.Pitches:
                    return string.Join(",", orderedNotes.Select(x => x.Pitch.ToString())) + ",";
                case PhraseTypeEnum.PitchDirection:
                    for (var j = 1; j < orderedNotes.Count; j++)
                    {
                        var value = Math.Sign(orderedNotes[j].Pitch - orderedNotes[j - 1].Pitch);
                        switch (value)
                        {
                            case -1:
                                asString += "-,";
                                break;
                            case 0:
                                asString += "0,";
                                break;
                            case 1:
                                asString += "+,";
                                break;
                            default:
                                throw new Exception("Que mierda me mandaron?");
                        }
                    }
                    return asString;
                default:
                    throw new Exception("Que mierda me mandaron?");
            }
        }

        /// <summary>
        /// We don't want phrases that are longer than 2 bars. If there is one, we break it in bar boundaries
        /// </summary>
        /// <param name="notes"></param>
        /// <param name="bars"></param>
        /// <param name="edgesSoFar"></param>
        /// <returns></returns>
        public static HashSet<long> BreakPhrasesThatAreTooLong(List<Note> notes, List<Bar> bars, HashSet<long> edgesSoFar)
        {
            var retObj = new HashSet<long>(edgesSoFar);
            var edgesAsList = edgesSoFar.ToList().OrderBy(x => x).ToList();
            for (int i = 0; i < edgesAsList.Count - 1; i++)
            {
                (var startBar, var startBeat) = GetBarAndBeatOfTick(bars, edgesAsList[i]);
                (var endBar, var endBeat) = GetBarAndBeatOfTick(bars, edgesAsList[i + 1]);
                if (startBar >= bars.Count) break;
                if (edgesAsList[i + 1] - edgesAsList[i] > bars[startBar].LengthInTicks * 2)
                {
                    for (var j = startBar + 1; j < endBar; j += 2)
                        retObj.Add(bars[j].TicksFromBeginningOfSong);
                }
            }
            return retObj;
        }
        public static (int, int) GetBarAndBeatOfTick(List<Bar> bars, long tick)
        {
            var barNo = bars.Where(b => b.TicksFromBeginningOfSong <= tick).Count();
            var beatLength = 4 * 96 / bars[barNo - 1].TimeSignature.Denominator;
            var beat = (int)(tick - bars[barNo - 1].TicksFromBeginningOfSong) / beatLength;
            return (barNo, beat);
        }
    }
}
