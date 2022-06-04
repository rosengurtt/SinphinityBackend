using Sinphinity.Models;
using SinphinityModel.Helpers;

namespace SinphinityProcMelodyAnalyser.BusinessLogic
{
        public  static partial class PhraseDetection
        {

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
            if (retObj.Where(x => x.DurationInTicks <= 0).Any())
            {

            }
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


    }
}
