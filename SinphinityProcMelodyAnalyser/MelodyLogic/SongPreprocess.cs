using Sinphinity.Models;
using SinphinityModel.Helpers;

namespace SinphinityProcMelodyAnalyser.MelodyLogic
{
    /// <summary>
    /// Before we can start working with phrases, we need to do the following
    /// 
    /// - remove the notes that are percussion notes  
    /// - remove the tracks that play chords
    /// - discretize the starts of the notes
    /// - remove duplicate notes
    /// - find which of the remaining tracks have multiple voices (for ex. a piano track where the left hand and the right hand play independent melodies)
    /// - in the ployphoinic voices assign a subvoice of 0 to the notes that belong to the lower voice and 1 to the notes of the higher voice
    /// - we accomodate the durations of the notes
    /// 
    /// We ignore any middle voices between the higher voice and the lowe voice
    /// 
    /// We return the resulting list of notes, that are now ready for analyzing their melodies.
    /// </summary>
    public static class SongPreprocess
    {

        /// <summary>
        /// Returns all notes that are part of melodies (not chords or percussion). When a track has more than 1 voice, it returns 2 "subvoices", a lower and higher ones
        /// that can be distinguished by the SubVoice field of the Note. When a track has a single voice, all notes have a subvoice of 0
        /// </summary>
        /// <param name="song"></param>
        /// <returns></returns>
        public static List<Note> ExtractMelodies(Song song)
        {
            var retObj = new List<Note>();
            var simplification = song.SongSimplifications[0];
            simplification.Notes = AddGuidToNotes(simplification.Notes);
            long tolerance = 4;
            var nonPercusionVoices = simplification.Notes.NonPercussionVoices();


            foreach (var voice in nonPercusionVoices)
            {
                var voiceNotes = simplification.Notes.Where(x => x.Voice == voice)
                                    .OrderBy(x => x.StartSinceBeginningOfSongInTicks)
                                    .ToList()
                                    .Clone();
                voiceNotes = DiscretizeTiming(voiceNotes);
                var processedVoiceNotes = new List<Note>();

                if (voiceNotes.Count < 10)
                    continue;
                // We don't extract melodies from tracks that play chords
                if (IsChordsTrack(voiceNotes))
                    continue;

                // If a voice has independent melodies, extract the lower and higher ones
                if (IsTrackPolyphonic(voiceNotes))
                {
                    voiceNotes = RemoveDuplicates(voiceNotes).OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();

                    foreach (var n in voiceNotes)
                    {
                        var neighboors = GetNeighboorsOfNote(voiceNotes, n);
                        (var averageLow, var average, var averageHigh) = GetNotesAverage(neighboors);
                        if (Math.Abs(n.Pitch - averageLow) < Math.Abs(n.Pitch - averageHigh))
                        {
                            // if there are notes (that are not ornaments) starting aprox at the same time with a higher pitch, ignore this note, it belongs to a middle voice
                            if (voiceNotes.Where(x => Math.Abs(x.StartSinceBeginningOfSongInTicks - n.StartSinceBeginningOfSongInTicks) < tolerance &&
                    x.DurationInTicks > 12 && n.DurationInTicks > 12 && n.Pitch > x.Pitch).Any())
                                continue;
                            else
                            {
                                n.SubVoice = 0;
                                processedVoiceNotes.Add(n);
                            }
                        }
                        else
                        {
                            // if there are notes (that are not ornaments) starting aprox at the same time with a lower pitch, ignore this note, it belongs to a middle voice
                            if (voiceNotes.Where(x => Math.Abs(x.StartSinceBeginningOfSongInTicks - n.StartSinceBeginningOfSongInTicks) < tolerance &&
                            x.DurationInTicks > 12 && n.DurationInTicks > 12 && n.Pitch < x.Pitch).Any())
                                continue;
                            else
                            {
                                n.SubVoice = 1;
                                processedVoiceNotes.Add(n);
                            }
                        }
                    }
                }
                else
                {
                    processedVoiceNotes = voiceNotes;
                    processedVoiceNotes.ForEach(x => x.SubVoice = 0);
                }
                processedVoiceNotes = RemoveChordNotes(processedVoiceNotes);
                processedVoiceNotes = MakeNotesStrictlyConsecutiveWithNoGaps(processedVoiceNotes);
                retObj = retObj.Concat(processedVoiceNotes).ToList();
            }
            return retObj;
        }
        /// <summary>
        /// If a voice is not polyphonic and has chords we keep the higher note of the chord and remove the rest
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        public static List<Note> RemoveChordNotes(List<Note> notes)
        {
            var retObj = notes.Clone();
            var notesToRemove = new HashSet<Note>();
            var tolerance = 3;
            foreach (var n in retObj)
            {
                var chordNotes = retObj.Where(x => Math.Abs(n.StartSinceBeginningOfSongInTicks - x.StartSinceBeginningOfSongInTicks) < tolerance
                     && n.Voice == x.Voice && n.SubVoice == x.SubVoice && (n.Pitch > x.Pitch || n.Guid != x.Guid));
                if (chordNotes.Any())
                {
                    notesToRemove = notesToRemove.Concat(chordNotes).ToHashSet();
                }
            }
            foreach (var note in notesToRemove)
            {
                retObj.Remove(note);
            }
            return retObj;
        }

        private static List<Note> AddGuidToNotes(List<Note> notes)
        {
            foreach (var n in notes)
                n.Guid = Guid.NewGuid();
            return notes;
        }

        private static List<Note> RemoveDuplicates(List<Note> notes)
        {
            var retObj = notes.Clone();
            var notesToRemove = new List<Note>();
            foreach (var n in retObj)
            {
                var dupl = retObj.Where(x => x.StartSinceBeginningOfSongInTicks == n.StartSinceBeginningOfSongInTicks && x.SubVoice == n.SubVoice)
                    .OrderByDescending(z => z.Pitch)
                    .OrderByDescending(y => y.Volume).ToList();
                if (dupl.Count > 1)
                {
                    for (var i = 1; i < dupl.Count; i++)
                    {
                        notesToRemove.Add(dupl[i]);
                    }
                }
            }
            foreach (var n in notesToRemove)
            {
                retObj.Remove(n);
            }
            return retObj;
        }

        /// <summary>
        /// We compute the total amount of time when notes are starting and ending together and we compare with the total duration of all notes
        /// We arbitrarily consider that the track consists of chords if the ratios is greater than 0.7
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        public static bool IsChordsTrack(List<Note> notes)
        {
            long totalChordsTime = 0;
            long totalTime = notes.Sum(x => x.DurationInTicks);
            long tolerance = 4;
            var computedNotes = new HashSet<string>();
            foreach (var note in notes.OrderBy(x => x.StartSinceBeginningOfSongInTicks))
            {
                var simultaneousNotes = notes.Where(x => Math.Abs(x.StartSinceBeginningOfSongInTicks - note.StartSinceBeginningOfSongInTicks) < tolerance &&
                     Math.Abs(x.EndSinceBeginningOfSongInTicks - note.EndSinceBeginningOfSongInTicks) < tolerance).ToList();
                foreach (var n in simultaneousNotes)
                {
                    if (simultaneousNotes.Count > 1 && !computedNotes.Contains(n.Guid.ToString()))
                    {
                        totalChordsTime += n.DurationInTicks;
                        computedNotes.Add(n.Guid.ToString());
                    }
                }
            }
            return totalChordsTime / (double)totalTime > 0.7;
        }

        /// <summary>
        /// We find the total time where 2 or more voices are played simultaneously and we compare it with the sum of the duration of all notes
        /// We arbitrarily consider the voice polyphonic if the ratio is greater than 10.5
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        public static bool IsTrackPolyphonic(List<Note> notes)
        {
            long totalTime = notes.Sum(x => x.DurationInTicks);
            long totalPolyphonyTime = 0;
            foreach (var note in notes.OrderBy(x => x.StartSinceBeginningOfSongInTicks))
            {
                var simultaneousNotes = notes.Where(x => x.StartSinceBeginningOfSongInTicks < note.EndSinceBeginningOfSongInTicks &&
                  x.EndSinceBeginningOfSongInTicks > note.StartSinceBeginningOfSongInTicks && x.Guid != note.Guid).ToList();
                totalPolyphonyTime += simultaneousNotes
                    .Sum(x => Math.Min(x.EndSinceBeginningOfSongInTicks, note.EndSinceBeginningOfSongInTicks) - Math.Max(x.StartSinceBeginningOfSongInTicks, note.StartSinceBeginningOfSongInTicks)) / 2;
            }
            return totalPolyphonyTime / (double)totalTime > 0.5;
        }

        /// <summary>
        /// Calculates the average pitch of all the notes, and then the average of 
        /// the notes that have a pitch above the average and the average of the notes
        /// under the average
        /// 
        /// When we calculate an average, rather than adding all the pitches and dividing by the number of notes, we take into account the time
        /// each pitch is played. So a pitch that is played for 96 ticks count twice that some pitch played for 48 ticks
        /// </summary>
        /// <param name="notes"></param>
        /// <returns>A tuple with the lower average, the total average and the higher average</returns>
        private static (double, double, double) GetNotesAverage(List<Note> notes)
        {
            if (notes.Sum(y => y.DurationInTicks) == 0){

            }
            var averagePitch = notes.Sum(x => x.Pitch * x.DurationInTicks) / notes.Sum(y => y.DurationInTicks);
            var notesOverAverage = notes.Where(x => x.Pitch > averagePitch).ToList();
            var notesUnderAverage = notes.Where(x => x.Pitch < averagePitch).ToList();

            var averageHigh = notesOverAverage.Count > 0 ?
                notesOverAverage.Sum(x => x.Pitch * x.DurationInTicks) / notesOverAverage.Sum(y => y.DurationInTicks) :
                averagePitch;
            var averageLow = notesUnderAverage.Count > 0 ?
                notesUnderAverage.Sum(x => x.Pitch * x.DurationInTicks) / notesUnderAverage.Sum(y => y.DurationInTicks) :
                averagePitch;
            return (averageLow, averagePitch, averageHigh);
        }

        /// <summary>
        /// Returns 30 notes that are close in time. It tries to get the 15 previous notes and the 10 following notes, 
        /// but if there are less than 15 previous notes it returns more following notes, to try to return always 20 notes
        /// The same if there are less than 15 following notes, it returns more previous notes
        /// </summary>
        /// <param name="notes"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        private static List<Note> GetNeighboorsOfNote(List<Note> notes, Note n)
        {
            var totalNeighboors = 30;
            var totalLeft = notes.Where(x => x.StartSinceBeginningOfSongInTicks < n.StartSinceBeginningOfSongInTicks).Count();
            var totalRight = notes.Where(x => x.StartSinceBeginningOfSongInTicks > n.StartSinceBeginningOfSongInTicks).Count();

            var numberNeighboorsLeft = totalLeft < totalNeighboors / 2 ? totalLeft : totalNeighboors / 2;
            var numberNeighboorsRight = totalRight < totalNeighboors / 2 ? totalRight : totalNeighboors / 2;
            if (numberNeighboorsLeft < totalNeighboors / 2)
                numberNeighboorsRight += totalNeighboors / 2 - numberNeighboorsLeft;
            if (numberNeighboorsRight < totalNeighboors / 2)
                numberNeighboorsLeft += totalNeighboors / 2 - numberNeighboorsRight;


            var neighboorsLeft = notes.Where(x => x.StartSinceBeginningOfSongInTicks < n.StartSinceBeginningOfSongInTicks)
                   .OrderByDescending(y => y.StartSinceBeginningOfSongInTicks)
                   .Take(numberNeighboorsLeft)
                   .ToList();

            var neighboorsRight = notes.Where(x => x.StartSinceBeginningOfSongInTicks > n.StartSinceBeginningOfSongInTicks)
                   .OrderBy(y => y.StartSinceBeginningOfSongInTicks)
                   .Take(numberNeighboorsRight)
                   .ToList();

            return neighboorsLeft.Concat(neighboorsRight).ToList();
        }

        /// <summary>
        /// When we have to phrases with a metric of 27,24, 21 and another with a metric of 22,26,24, they are really the same thing and we should write them as 24,24,24
        /// This function changes the starts of the notes 
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        private static List<Note> DiscretizeTiming(List<Note> notes)
        {
            if (notes.Count < 2)
                return notes;
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
            long lowerValue = Math.Max(middle - middle % unit, left);
            long higherValue = Math.Min(lowerValue + unit, right);
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
        /// Extends or shortens the durations of notes so each note starts exactly where the previous finished
        /// The notes parameter are expected to be all in the same voice, but it can be a polyphonic voice
        /// with a lower subvoice and a higher one
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        public static List<Note> MakeNotesStrictlyConsecutiveWithNoGaps(List<Note> notes)
        {
            var lowerVoice = notes.Where(x => x.SubVoice == 0).ToList().Clone();
            var higherVoice = notes.Where(x => x.SubVoice == 1).ToList().Clone();
            for (int i = 0; i < lowerVoice.Count - 1; i++)
            {
                if (lowerVoice[i].StartSinceBeginningOfSongInTicks== lowerVoice[i + 1].StartSinceBeginningOfSongInTicks)
                {

                }
                lowerVoice[i].EndSinceBeginningOfSongInTicks = lowerVoice[i + 1].StartSinceBeginningOfSongInTicks;
            }
            for (int i = 0; i < higherVoice.Count - 1; i++)
            {

                if (higherVoice[i].StartSinceBeginningOfSongInTicks == higherVoice[i + 1].StartSinceBeginningOfSongInTicks)
                {

                }
                higherVoice[i].EndSinceBeginningOfSongInTicks = higherVoice[i + 1].StartSinceBeginningOfSongInTicks;
            }
            return lowerVoice.Concat(higherVoice).OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();
        }
    }
}
