﻿using Sinphinity.Models;
using SinphinityModel;
using System.Collections.Generic;

namespace SinphinityMelodyAnalizer.BusinessLogic
{
    public static class MelodyFinder
    {
        public static List<(PhraseMetric, PhrasePitches)> FindAllPhrases(List<Note> notes, List<Bar> bars)
        {
            var retObj = new List<(PhraseMetric, PhrasePitches)>();

            var voices = notes.Select(n => n.Voice).Distinct().OrderBy(v => v).ToList();
            foreach (var voice in voices)
            {
                var voiceNotes = notes.Where(x => x.Voice == voice).OrderBy(y => y.StartSinceBeginningOfSongInTicks).ThenByDescending(z => z.Pitch).ToList();
                var phraseEdges = GetPhrasesEdges(voiceNotes, bars).ToList();
                for (int i = 0; i < phraseEdges.Count - 1; i++)
                {
                    var phrase = GetPhraseBetweenEdges(voiceNotes, phraseEdges[i], phraseEdges[i + 1]);
                    if (phrase.Item1 != null && phrase.Item2 != null)
                        retObj.Add((phrase.Item1, phrase.Item2));
                }
            }
            return retObj;
        }

        private static (PhraseMetric?, PhrasePitches?) GetPhraseBetweenEdges(List<Note> notes, long start, long end)
        {
            var phraseNotes = notes
                .Where(x => x.StartSinceBeginningOfSongInTicks >= start && x.StartSinceBeginningOfSongInTicks < end)
                .OrderBy(y => y.StartSinceBeginningOfSongInTicks)
                .ToList();
            if (phraseNotes.Count > 0)
                return (new PhraseMetric(phraseNotes), new PhrasePitches(phraseNotes));
            return (null, null);
        }

        /// <summary>
        /// Analyzes the metric of the song to find the points where phrases start and end
        /// Returns a list with the locations represented as the number of ticks since the beginning of the song 
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        private static HashSet<long> GetPhrasesEdges(List<Note> notes, List<Bar> bars)
        {
            var cleanedNotes = RemoveHarmony(notes, bars);
            // First we find the points that separate the phrases in the song
            var retObj = GetEdgesOfGroupsOfNotesWithIdenticalDuration(cleanedNotes, bars, new HashSet<long>());
            retObj = GetEdgesOfSilencesAndLongNotes(notes, bars, retObj);
            retObj = BreakPhrasesThatAreTooLong(notes, bars, retObj);

            return retObj;
        }

        /// <summary>
        /// When there are 2 or more notes starting at the same time, we keep only he higher one and remove the rest
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        private static List<Note> RemoveHarmony(List<Note> notes, List<Bar> bars)
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
        /// Looks for group of consecutive notes of the same duration that have these properties
        /// - there are at least 3 notes
        /// - there are no more than 16 notes
        /// - the total duration of the group of notes is at leat 1 beat
        /// - the total duration of the group of notes is not more than 1 bar
        /// - when we have too many consecutive notes with the same duration, we separate them in points where the ticks from start are multiple of 48 or 96
        /// - we are greedy: if we have n consecutive notes that satisfy the previous condition, and the next note is of the same duration as the
        ///   previous one, and after adding it we still satisfy the conditions, we add it
        /// </summary>
        /// <param name="notes"></param>
        /// <param name="edgesSoFar"></param>
        /// <returns></returns>
        private static HashSet<long> GetEdgesOfGroupsOfNotesWithIdenticalDuration(List<Note> notes, List<Bar> bars, HashSet<long> edgesSoFar)
        {
            var retObj = new HashSet<long>(edgesSoFar);
            var minAllowableConsecutiveNotes = 3;
            var maxAllowableConsecutiveNotes = 16;

            var groups = GetGroupsOfConsecutiveNotesWithSameDuration(notes);


            foreach (var group in groups)
            {
                var groupStartTick = group.Item1;
                var numberOfConsecutiveNotes = group.Item2;
                int noteDuration = group.Item3;
                var groupEndTick = groupStartTick + numberOfConsecutiveNotes * noteDuration;
                (var startBar, var startBeat) = GetBarAndBeatOfTick(bars, groupStartTick);
                var minAllowableTotalDuration = bars[startBar].LengthInTicks / bars[startBar].TimeSignature.Numerator;
                var maxAllowableTotalDuration = bars[startBar].LengthInTicks;
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
                for (var j = startBar + 1; j <= endBar; j++)
                    retObj.Add(bars[j - 1].TicksFromBeginningOfSong);

            }

            foreach (var edge in retObj)
            {
                (var bar, var beat) = GetBarAndBeatOfTick(bars, edge);
            }
            return retObj;
        }

        /// <summary>
        /// When there are large spaces with no notes played, we consider this a boundary of a phrase
        /// </summary>
        /// <param name="notes"></param>
        /// <param name="bars"></param>
        /// <param name="edgesSoFar"></param>
        /// <returns></returns>
        private static HashSet<long> GetEdgesOfSilencesAndLongNotes(List<Note> notes, List<Bar> bars, HashSet<long> edgesSoFar)
        {
            var retObj = new HashSet<long>(edgesSoFar);
            for (int i = 0; i < notes.Count - 1; i++)
            {
                (var currentBar, var currentBeat) = GetBarAndBeatOfTick(bars, notes[i].StartSinceBeginningOfSongInTicks);
                // if 2 consecutive notes are serparated too much, add their starts
                int whatIsConsideredLTooMuch = bars[currentBar - 1].LengthInTicks;
                if (notes[i + 1].StartSinceBeginningOfSongInTicks - notes[i].StartSinceBeginningOfSongInTicks >= whatIsConsideredLTooMuch)
                {
                    retObj.Add(notes[i].StartSinceBeginningOfSongInTicks);
                    retObj.Add(notes[i + 1].StartSinceBeginningOfSongInTicks);
                }
            }
            return retObj;
        }

        /// <summary>
        /// Looks for groups of consecutive notes with the same duration and for each returns the point where it starts, the number of consecutive notes and
        /// the duration of the note
        /// </summary>
        /// <param name="notes"></param>
        /// <param name=""></param>
        /// <returns></returns>
        private static List<(long, int, int)> GetGroupsOfConsecutiveNotesWithSameDuration(List<Note> notes)
        {
            var retObj = new List<(long, int, int)>();
            long currentGroupStart = notes[0].StartSinceBeginningOfSongInTicks;
            var currentGroupNoteDuration = notes[0].DurationInTicks;
            var currentConsecutiveNotes = 0;
            foreach (var note in notes)
            {
                if (note.DurationInTicks == currentGroupNoteDuration)
                {
                    currentConsecutiveNotes++;
                }
                else
                {
                    retObj.Add((currentGroupStart, currentConsecutiveNotes, currentGroupNoteDuration));
                    currentConsecutiveNotes = 1;
                    currentGroupStart = note.StartSinceBeginningOfSongInTicks;
                    currentGroupNoteDuration = note.DurationInTicks;
                }
            }
            return retObj;
        }

        /// <summary>
        /// We don't want phrases that are longer than 1 bar. If there is one, we break it in bar boundaries
        /// </summary>
        /// <param name="notes"></param>
        /// <param name="bars"></param>
        /// <param name="edgesSoFar"></param>
        /// <returns></returns>
        private static HashSet<long> BreakPhrasesThatAreTooLong(List<Note> notes, List<Bar> bars, HashSet<long> edgesSoFar)
        {
            var retObj = new HashSet<long>(edgesSoFar);
            for (int i = 0; i < edgesSoFar.Count - 1; i++)
            {
                var edgesAsList = edgesSoFar.ToList().OrderBy(x => x).ToList();
                (var startBar, var startBeat) = GetBarAndBeatOfTick(bars, edgesAsList[i]);
                (var endBar, var endBeat) = GetBarAndBeatOfTick(bars, edgesAsList[i + 1]);
                if (edgesAsList[i + 1] - edgesAsList[i] > bars[startBar].LengthInTicks * 2)
                {
                    for (var j = startBar + 1; j <= endBar; j++)
                        retObj.Add(bars[j - 1].TicksFromBeginningOfSong);
                }
            }
            return retObj;
        }

        private static (int, int) GetBarAndBeatOfTick(List<Bar> bars, long tick)
        {
            var barNo = bars.Where(b => b.TicksFromBeginningOfSong <= tick).Count();
            var beatLength = 4 * 96 / bars[barNo - 1].TimeSignature.Denominator;
            var beat = (int)(tick - bars[barNo - 1].TicksFromBeginningOfSong) / beatLength;
            return (barNo, beat);
        }
    }
}
