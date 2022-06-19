using Newtonsoft.Json;
using Sinphinity.Models;
using SinphinityModel.Helpers;
using SinphinityProcMelodyAnalyser.Models;
using System.Text.RegularExpressions;

namespace SinphinityProcMelodyAnalyser.BusinessLogic
{
    public static partial class PhraseDetection
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
                if (hasEmbellishments && phraseWithoutEmbellishmentNotes.Count <= 3)
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
            edgesSoFar = BreakPhrasesThatAreTooLong(notes, bars, edgesSoFar);
            return edgesSoFar;
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
        /// If after all, there are still phrases that are too long (more than 2 bars) and/or have too many notes, break them arbitrarily at bar beginnings recursively until there
        /// are no phrases that are too long an/or have too many notes
        /// We break at bar edges only if there is a note at the beginning of the bar
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
                var startTick = edgesAsList[i];
                var endTick = edgesAsList[i + 1];
                if (MustIntervalBeBroken(notes, bars, startTick, endTick))
                    retObj = retObj.Union(BreakInterval(notes, bars, startTick, endTick)).ToHashSet();
            }
            return retObj;
        }
        /// <summary>
        /// Checks if a phrase would be too long and/or have too many notes and must therefore be cut
        /// </summary>
        /// <param name="notes"></param>
        /// <param name="bars"></param>
        /// <param name="startTick"></param>
        /// <param name="endTick"></param>
        /// <returns></returns>
        private static bool MustIntervalBeBroken(List<Note> notes, List<Bar> bars, long startTick, long endTick)
        {
            var notesInInterval = notes.Where(x => x.StartSinceBeginningOfSongInTicks >= startTick && x.StartSinceBeginningOfSongInTicks < endTick).Count();

            (var startBar, var startBeat) = GetBarAndBeatOfTick(bars, startTick);
            (var endBar, var endBeat) = GetBarAndBeatOfTick(bars, endTick);
            if (startBar >= bars.Count) 
                return false;

            if ((endTick - startTick) * notesInInterval > bars[startBar].LengthInTicks * 2 * 20)
                return true;
            return false;
        }

        private static HashSet<long> BreakInterval(List<Note> notes, List<Bar> bars, long startTick, long endTick)
        {
            var retObj = new HashSet<long>();
            (var startBar, var startBeat) = GetBarAndBeatOfTick(bars, startTick);
            (var endBar, var endBeat) = GetBarAndBeatOfTick(bars, endTick);
            // if startBar and endBar are contiguos, don't break the interval
            if (startBar + 1 == endBar)
                return retObj;
            var middleBar = (startBar + endBar) / 2;
            if (middleBar == startBar)
                middleBar += 1;

            var startMiddleBar = bars[middleBar - 1].TicksFromBeginningOfSong;
            retObj.Add(startMiddleBar);

            // Recursively break left and right intervals
            if (MustIntervalBeBroken(notes, bars, startTick, startMiddleBar))
                retObj = retObj.Union(BreakInterval(notes, bars, startTick, startMiddleBar)).ToHashSet();
            if (MustIntervalBeBroken(notes, bars, startMiddleBar, endTick))
                retObj = retObj.Union(BreakInterval(notes, bars, startMiddleBar, endTick)).ToHashSet();
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
