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

            if (phraseNotes.Count > 0)
            {
                (var hasEmbellishments, var phraseWithoutEmbellishmentNotes) = EmbelishmentsDetection.GetPhraseWithoutEmbellishments(phraseNotes);
                var metrics = hasEmbellishments ? new PhraseMetrics(phraseWithoutEmbellishmentNotes, start, end) : new PhraseMetrics(phraseNotes, start, end);
                var pitches = hasEmbellishments ? new PhrasePitches(phraseWithoutEmbellishmentNotes) : new PhrasePitches(phraseNotes);
                var embellishedMetrics = hasEmbellishments ? new PhraseMetrics(phraseNotes, start, end) : null;
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
                    retObj.Add(orderedNotes[i]);
            }
            return retObj;
        }

        /// <summary>
        /// When we have to phrases with a metric of 27,24, 21 and another with a metric of 22,26,24, they are really the same thing and we should write them as 24,24,24
        /// This function changes the starts of the notes 
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        public static List<Note> DiscretizeTiming(List<Note> notes, List<Bar> bars)
        {
            var retObj = new List<Note>();
            retObj.Add((Note)notes[0].Clone());
            var orderedNotes = notes.OrderBy(x => x.StartSinceBeginningOfSongInTicks).ThenByDescending(y => y.Pitch).ToList();

            for (var i = 1; i < orderedNotes.Count - 1; i++)
            {
                var left = orderedNotes[i - 1].StartSinceBeginningOfSongInTicks;
                var middle = orderedNotes[i].StartSinceBeginningOfSongInTicks;
                var right = orderedNotes[i + 1].StartSinceBeginningOfSongInTicks;
                var calculatedBestPoint = ClosestDiscretePoint(left, middle, right);

                var note = (Note)notes[i].Clone();
                note.StartSinceBeginningOfSongInTicks = calculatedBestPoint;
                retObj.Add(note);
            }
            retObj.Add((Note)notes[notes.Count - 1].Clone());
            return retObj;
        }


        private static long ClosestDiscretePoint(long left, long middle, long right)
        {
            long minDuration = Math.Min(right - middle, middle - left);
            long unit = GetUnit(minDuration);
            if (middle % unit == 0) return middle;
            long lowerValue = middle - middle % unit;
            long higherValue = lowerValue + unit;
            return (middle - lowerValue < higherValue - middle) ? lowerValue : higherValue;
        }
        private static long GetUnit(long duration)
        {
            for (var i = 1; i < 5; i++)
            {
                if (duration == 3 * Math.Pow(2, i))
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
            var notesCleaned = GetNotesWithSilencesRemoved(notes);
            var candidates = new List<Note>();
            // We find the longest note in each bar. We look for a note that is longer to all other notes in the same bar
            // We store these longest notes in the candidates list
            for (var i = 0; i < bars.Count - 2; i++)
            {
                var neighboors = notesCleaned.Where(x => x.StartSinceBeginningOfSongInTicks >= bars[i].TicksFromBeginningOfSong &&
                                                    x.StartSinceBeginningOfSongInTicks < bars[i + 1].TicksFromBeginningOfSong).ToList();
                if (neighboors == null || neighboors.Count == 0) continue;
                var longestNoteDuration = neighboors.Max(x => x.DurationInTicks);
                var notesWithLongestDuration = neighboors.Where(x => x.DurationInTicks == longestNoteDuration).ToList();

                var secondLongestNote = neighboors.Where(x => x.DurationInTicks < longestNoteDuration).FirstOrDefault();
                if (notesWithLongestDuration.Count == 1 && secondLongestNote != null && longestNoteDuration > secondLongestNote.DurationInTicks)
                {
                    var longestNote = neighboors.Where(x => x.DurationInTicks == longestNoteDuration).FirstOrDefault();
                    retObj.Add(longestNote.EndSinceBeginningOfSongInTicks);
                }
            }
            return retObj;
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
            var edges = edgesSoFar.ToList().OrderBy(x => x).ToList();
            for (var i = 0; i < edges.Count - 1; i++)
            {
                var intervalNotes = notes.Where(x => x.StartSinceBeginningOfSongInTicks >= edges[i] && x.StartSinceBeginningOfSongInTicks < edges[i + 1]).ToList();
                (long? start, long? end) = GetRepeatingPatternSection(intervalNotes);
                if (start.HasValue && end.HasValue)
                {
                    edgesSoFar.Add(start.Value);
                    edgesSoFar.Add(end.Value);
                }
            }
            return edgesSoFar;
        }
        /// <summary>
        /// Given a group of consecutive notes, looks for a subset of them that consist of a repeating pattern and returns the start and end of the repeating pattern section
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        private static (long?, long?) GetRepeatingPatternSection(List<Note> notes)
        {
            var orderedNotes = notes.OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();
            if (notes.Count < 8) return (null, null);
            var asString = GetNotesAsString(notes);
            for (var i = 1; i < notes.Count / 2; i++)
            {
                var pat = string.Concat(Enumerable.Repeat("[0-9]+,[-]?[0-9]+;", i));
                foreach (Match match in Regex.Matches(asString, pat))
                {
                    for (var j = notes.Count / i; j > 2; j--)
                    {
                        var pat2 = string.Concat(Enumerable.Repeat(match.Value,j));

                        if (asString.Contains(pat2))
                        {
                            var start = asString.IndexOf(pat2);
                            var notesBeforeStart = asString.Substring(0, start).Count(x => x == ';');
                            var notesInRepeatedPattern = pat2.Count(x => x == ';');
                            if (notesInRepeatedPattern > 5)
                                return (orderedNotes[notesBeforeStart].StartSinceBeginningOfSongInTicks, orderedNotes[notesBeforeStart + notesInRepeatedPattern].StartSinceBeginningOfSongInTicks);
                       }
                    }
                }
            }
            return (null, null);
        }
        /// <summary>
        /// Creates strings like 48,3;24,-1;24,2
        /// that represent consecutive relative notes, using a comma to separate duration from pitch and semicolon between consecutive notes
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        private static string GetNotesAsString(List<Note> notes)
        {
            var orderedNotes = notes.OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();
            var asString = $"";
            for (var i = 0; i < orderedNotes.Count - 1; i++)
            {
                asString += $"{orderedNotes[i + 1].StartSinceBeginningOfSongInTicks - orderedNotes[i].StartSinceBeginningOfSongInTicks},{orderedNotes[i + 1].Pitch - orderedNotes[i].Pitch};";
            }
            return asString;
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
