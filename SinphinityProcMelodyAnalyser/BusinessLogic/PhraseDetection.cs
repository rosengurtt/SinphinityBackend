using Newtonsoft.Json;
using Sinphinity.Models;
using SinphinityModel.Helpers;
using SinphinityProcMelodyAnalyser.Models;

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
                var metrics = hasEmbellishments? new PhraseMetrics(phraseWithoutEmbellishmentNotes, start, end): new PhraseMetrics(phraseNotes, start, end);
                var pitches = hasEmbellishments ? new PhrasePitches(phraseWithoutEmbellishmentNotes): new PhrasePitches(phraseNotes);
                var embellishedMetrics = hasEmbellishments ? new PhraseMetrics(phraseNotes, start, end) : null;
                var embellishedPitches = hasEmbellishments ? new PhrasePitches(phraseNotes) : null;
                var retObj = new PhraseInfo
                {
                    Location = new SongLocation(songId, voice, start, bars),
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

            var retObj = GetEdgesOfSilencesAndLongNotes(notes, bars, edgesSoFar);
            retObj = GetEdgesOfGroupsOfNotesEvenlySpaced(notes, bars, retObj);
            retObj = BreakPhrasesThatAreTooLong(notes, bars, retObj);
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
            long averageDuration = (right - left) / 2;
            long unit = GetUnit(averageDuration);
            if (middle % unit == 0) return middle;
            long lowerValue = middle - middle % unit;
            long higherValue = lowerValue + unit;
            return (middle - lowerValue < higherValue - middle) ? lowerValue : higherValue;
        }
        private static long GetUnit(long duration)
        {
            long candidate = duration * 2 / 3;
            if (candidate < 9) return 6;
            if (candidate < 15) return 12;
            if (candidate < 19) return 16;
            if (candidate < 30) return 24;
            if (candidate < 35) return 32;
            return 48;
        }

        /// <summary>
        /// Looks for group of consecutive notes evenly spaced (that is: the distance in ticks between consecutive notes is constant) that have these properties
        /// - there are at least 3 notes
        /// - there are no more than 16 notes
        /// - the total duration of the group of notes is at leat 1 beat
        /// - the total duration of the group of notes is not more than 2 bars
        /// - when we have too many consecutive notes with the same duration, we separate them in points where the ticks from start are multiple of 48 or 96
        /// - we are greedy: if we have n consecutive notes that satisfy the previous condition, and the next note is of the same duration as the
        ///   previous one, and after adding it we still satisfy the conditions, we add it
        /// </summary>
        /// <param name="notes"></param>
        /// <param name="edgesSoFar"></param>
        /// <returns></returns>
        public static HashSet<long> GetEdgesOfGroupsOfNotesEvenlySpaced(List<Note> notes, List<Bar> bars, HashSet<long> edgesSoFar)
        {
            var retObj = new HashSet<long>(edgesSoFar);
            var minAllowableConsecutiveNotes = 3;
            var maxAllowableConsecutiveNotes = 16;

            var groups = GetGroupsOfConsecutiveNotesEvenlySpaced(notes);


            foreach (var group in groups)
            {
                var groupStartTick = group.Item1;
                var numberOfConsecutiveNotes = group.Item2;
                int noteDuration = group.Item3;
                var groupEndTick = groupStartTick + numberOfConsecutiveNotes * noteDuration;
                (var startBar, var startBeat) = GetBarAndBeatOfTick(bars, groupStartTick);
                if (startBar >= bars.Count) break;
                var minAllowableTotalDuration = bars[startBar].LengthInTicks * 2 / bars[startBar].TimeSignature.Numerator;
                var maxAllowableTotalDuration = 3 * bars[startBar].LengthInTicks;
                // if too short, ignore
                if (numberOfConsecutiveNotes < minAllowableConsecutiveNotes ||
                    numberOfConsecutiveNotes * noteDuration < minAllowableTotalDuration)
                    continue;
                // if right size, add
                if (numberOfConsecutiveNotes <= maxAllowableConsecutiveNotes &&
                  numberOfConsecutiveNotes * noteDuration <= maxAllowableTotalDuration)
                {
                    retObj.Add(groupStartTick);
                    retObj.Add(groupStartTick + numberOfConsecutiveNotes * noteDuration);
                }

                // if too long, break at appropriate points
                // endBar is the last bar that has notes of this group
                (var endBar, var endBeat) = GetBarAndBeatOfTick(bars, groupEndTick);
                retObj.Add(groupStartTick);
                for (var j = startBar + 1; j < endBar; j += 2)
                    retObj.Add(bars[j].TicksFromBeginningOfSong);

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
            for (var i = 0; i < bars.Count - 2; i += 2)
            {
                var neighboors = notesCleaned.Where(x => x.StartSinceBeginningOfSongInTicks >= bars[i].TicksFromBeginningOfSong && 
                                                    x.StartSinceBeginningOfSongInTicks < bars[i + 2].TicksFromBeginningOfSong).ToList();
                if (neighboors == null || neighboors.Count == 0) continue;
                var longestNoteDuration = neighboors.Max(x => x.DurationInTicks);
                var notesWithLongestDuration = neighboors.Where(x => x.DurationInTicks == longestNoteDuration).ToList();

                var secondLongestNote = neighboors.Where(x => x.DurationInTicks < longestNoteDuration).FirstOrDefault();
                if (secondLongestNote== null || longestNoteDuration > secondLongestNote.DurationInTicks)
                {
                    var longestNote = neighboors.Where(x => x.DurationInTicks == longestNoteDuration).FirstOrDefault();
                    candidates.Add(longestNote);
                }
            }
            // We now check if we should remove some of these candidates
            // When 2 candidates are close and one is much longer than the other we remove the shorter one
            var candidatesToBeRemoved = new List<Note>();
            for (int i = 0; i < candidates.Count - 1; i++)
            {
                (var bar, var beat) = GetBarAndBeatOfTick(bars, candidates[i].StartSinceBeginningOfSongInTicks);
                var barlength = bars[bar - 1].LengthInTicks;
                if (candidates[i + 1].StartSinceBeginningOfSongInTicks - candidates[i].StartSinceBeginningOfSongInTicks < barlength * 0.75)
                {
                    if (candidates[i].DurationInTicks >= 1.5 * candidates[i + 1].DurationInTicks)
                        candidatesToBeRemoved.Add(candidates[i + 1]);
                    else if (candidates[i + 1].DurationInTicks >= 1.5 * candidates[i].DurationInTicks)
                        candidatesToBeRemoved.Add(candidates[i]);
                }
            }
            for (int i = 0; i < candidatesToBeRemoved.Count; i++)
            {
                candidates.Remove(candidatesToBeRemoved[i]);
            }
            foreach(var n in candidates)
            {
                retObj.Add(n.EndSinceBeginningOfSongInTicks);
            }
            return retObj;
        }
  

        private static List<Note> GetNotesWithSilencesRemoved(List<Note> notes)
        {
            var retObj = notes.Clone();
            for (int i=0; i < retObj.Count-1; i++)
            {
                retObj[i].EndSinceBeginningOfSongInTicks = retObj[i + 1].StartSinceBeginningOfSongInTicks;
            }
            return retObj.OrderBy(x=>x.StartSinceBeginningOfSongInTicks).ToList();
        }


        /// <summary>
        /// Looks for groups of consecutive notes evenly spaced (meaning that the distance in ticks between consecutive notes is constant) and for each 
        /// returns the point where it starts, the number of consecutive notes and the duration of the note
        /// Duration here is not the actual duration of the note, but the separation in ticks from the start of the note to the start of the following note
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
            return retObj;
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
