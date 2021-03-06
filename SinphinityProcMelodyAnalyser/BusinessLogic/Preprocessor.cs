using Sinphinity.Models;
using SinphinityModel.Helpers;

namespace SinphinityProcMelodyAnalyser.BusinessLogic
{
    public static class Preprocessor
    {

        public static List<Note> RemoveDrumsAndChordsVoices(List<Note> notes)
        {
            var voices = notes.Select(n => n.Voice).Distinct().OrderBy(v => v).ToList();
            var voicesToRemove =new List<int>();
            foreach (var voice in voices)
            {
                var voiceNotes = notes.Where(x => x.Voice == voice).ToList();
                if (voiceNotes.FirstOrDefault().IsPercussion)
                    voicesToRemove.Add(voice);
                if (GetPercentageOfChordNotes(voiceNotes)>70)
                    voicesToRemove.Add(voice);
            }
            return notes.Where(x => !voicesToRemove.Contains(x.Voice)).ToList();
        }

        /// <summary>
        /// A bass voice may have melodies but in rock it often doesn't
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        public static List<Note> RemoveBoringBassVoices(List<Note> notes)
        {
            var voices = notes.Select(n => n.Voice).Distinct().OrderBy(v => v).ToList();
            var orderedNotes = notes.OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();
            var voicesToRemove =new List<int>();
            foreach (var voice in voices)
            {
                var voiceNotes = notes.Where(x => x.Voice == voice).ToList();
                if (voiceNotes.Average(x => x.Pitch) < 55)
                {
                    var countPitchUnchanged = 0;
                    for (var i = 0; i < orderedNotes.Count - 1; i++)
                    {
                        if (orderedNotes[i].Pitch == orderedNotes[i + 1].Pitch)
                            countPitchUnchanged++;
                        if (i > 0 && orderedNotes[i].Pitch != orderedNotes[i + 1].Pitch && orderedNotes[i - 1].Pitch == orderedNotes[i].Pitch)
                            countPitchUnchanged++;
                    }
                    if (countPitchUnchanged * 100 > voiceNotes.Count * 50)
                        voicesToRemove.Add(voice);
                }
            }
            return notes.Where(x => !voicesToRemove.Contains(x.Voice)).ToList();
        }

        private static int GetPercentageOfChordNotes(List<Note> notes)
        {
            var totalNotesTime = 0;
            var totalSyncNotesTime = 0;
            foreach (var n in notes)
            {
                totalNotesTime += n.DurationInTicks;
                if (notes.Where(x=> AreNotesSyncrhonous(n, x)).Count() > 2)
                {
                    totalSyncNotesTime += n.DurationInTicks;
                }
            }
            return totalSyncNotesTime * 100 / totalNotesTime;
        }
        private static bool AreNotesSyncrhonous(Note n1, Note n2)
        {
            if (Math.Abs(n1.StartSinceBeginningOfSongInTicks - n2.StartSinceBeginningOfSongInTicks) < 4 && Math.Abs(n1.EndSinceBeginningOfSongInTicks - n2.EndSinceBeginningOfSongInTicks) < 4)
                return true;
            return false;
        }
        /// <summary>
        /// If a track has more than 1 voice (for ex. a piano track where each hands plays an independent voice), it splits it in 2
        /// If a track is playing chords, it leaves it unchanged
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        public static List<Note> SplitMultiVoiceTracks(List<Note> notes)
        {
            var retObj = notes.Clone();
            var voices = notes.Select(n => n.Voice).Distinct().OrderBy(v => v).ToList();
            foreach (var voice in voices)
            {
                var voiceNotes = notes.Where(x => x.Voice == voice).OrderBy(y => y.StartSinceBeginningOfSongInTicks).ThenByDescending(z => z.Pitch).ToList();
                if ((voiceNotes[0].IsPercussion) || !IsTrackMultiVoice(voiceNotes)) continue;
                (var upper, var lower) = SplitVoiceIn2(voiceNotes);
                voiceNotes.ForEach(x => retObj.Remove(x));
                upper.ForEach(x => retObj.Add(x));
                lower.ForEach(x => x.Voice += (byte)voices.Count);
                lower.ForEach(x => retObj.Add(x));
            }
            return retObj;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="notes"></param>
        /// <param name="polyphonycPercentage">The percentage of time that many notes have to be playing simultaneously to be considered a polyphonic voice</param>
        /// <returns></returns>
        private static bool IsTrackMultiVoice(List<Note> notes, int polyphonycPercentage = 50, int chordPercentage = 90)
        {
            long totalSoundTime = 0;
            long totalPolyphonycTime = 0;
            long totalChordTime = 0;
            for (var i = 0; i < notes.Count - 1; i++)
            {
                totalSoundTime += notes[i].DurationInTicks;
                var polyphonicNotes = notes.Where(x => x.StartSinceBeginningOfSongInTicks < notes[i].EndSinceBeginningOfSongInTicks &&
                  x.EndSinceBeginningOfSongInTicks > notes[i].StartSinceBeginningOfSongInTicks).ToList();
                totalPolyphonycTime += Intersection(notes[i], notes);
                var chordNotes = notes.Where(x => Math.Abs(x.StartSinceBeginningOfSongInTicks - notes[i].StartSinceBeginningOfSongInTicks) < 3 &&
                                                Math.Abs(x.EndSinceBeginningOfSongInTicks - notes[i].EndSinceBeginningOfSongInTicks) < 3).ToList();
                if (chordNotes.Count > 1)
                    totalChordTime += notes[i].DurationInTicks;
            }
            return totalPolyphonycTime * 100 > totalSoundTime * polyphonycPercentage && totalChordTime * 100 < totalSoundTime * chordPercentage;
        }

        /// <summary>
        /// Given a note finds the total time during the duration of the note where other notes are playing simultaneously
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="notes"></param>
        /// <returns></returns>
        private static long Intersection(Note note, List<Note> notes)
        {
            var intervals = new List<(long, long)>();
            foreach (var n in notes)
            {
                var startInt = Math.Max(note.StartSinceBeginningOfSongInTicks, n.StartSinceBeginningOfSongInTicks);
                var endInt = Math.Min(note.EndSinceBeginningOfSongInTicks, n.EndSinceBeginningOfSongInTicks);
                var existingInt = intervals.Where(x => x.Item1 < endInt && x.Item2 > startInt).ToList();
                if (existingInt.Count > 0)
                {
                    var newStart = Math.Min(startInt, n.StartSinceBeginningOfSongInTicks);
                    var newEnd = Math.Max(endInt, n.EndSinceBeginningOfSongInTicks);
                    var intervalsToDelete = intervals.Where(x => x.Item1 >= newStart && x.Item2 <= newEnd).ToList();
                    foreach (var i in intervalsToDelete)
                        intervals.Remove(i);
                    intervals.Add((newStart, newEnd));
                }
            }
            long totalPolyphony = 0;
            foreach (var i in intervals)
                totalPolyphony += (i.Item2 - i.Item1);
            return totalPolyphony;
        }


        private static (List<Note>, List<Note>) SplitVoiceIn2(List<Note> notes)
        {
            var upperVoice = new List<Note>();
            var lowerVoice = new List<Note>();
            foreach (var n in notes)
            {
                if (notes.Where(x => x.StartSinceBeginningOfSongInTicks > n.EndSinceBeginningOfSongInTicks &&
                x.EndSinceBeginningOfSongInTicks > n.StartSinceBeginningOfSongInTicks && x.Pitch > n.Pitch).Count() == 0)
                    upperVoice.Add(n);
                if (notes.Where(x => x.StartSinceBeginningOfSongInTicks > n.EndSinceBeginningOfSongInTicks &&
                x.EndSinceBeginningOfSongInTicks > n.StartSinceBeginningOfSongInTicks && x.Pitch < n.Pitch).Count() == 0)
                    lowerVoice.Add(n);
            }
            return (upperVoice, lowerVoice);
        }
    }
}