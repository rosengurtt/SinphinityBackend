using Sinphinity.Models;
using SinphinityModel.Helpers;

namespace SinphinityProcMelodyAnalyser.BusinessLogic
{
    public static class EmbelishmentsDetection
    {
        /// <summary>
        /// If a phrase has embellishments returns a true in the first item and the version of the phrase without embellishments in the second item
        /// If it has no embelishments returns a false in the first item and an empty list
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        public static (bool, List<Note>) GetPhraseWithoutEmbellishments(List<Note> notes)
        {
            var areThereEmbellishments = false;
            // If there are no notes shorter than a sixteenth, then there are no embellisments
            if (!notes.Where(x => x.DurationInTicks < 24).Any()) return (false, new List<Note>());

            // If all notes are 32nds or shorter, then it is a rapid passage and there are no embelishments
            if (!notes.Where(x => x.DurationInTicks >= 12).Any()) return (false, new List<Note>());

            var retObj = notes.Clone().OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();

            var trillNotes = GetTrills(retObj);
            if (trillNotes.Count > 0)
            {
                areThereEmbellishments = true;
                retObj = RemoveTrills(retObj, trillNotes);
            }

            var shortEmbellishments = GetShortEmbellishments(retObj);
            if (shortEmbellishments.Count > 0)
            {
                areThereEmbellishments = true;
                retObj = RemoveShortEmbelishments(retObj, shortEmbellishments);
            }

            if (retObj.Count == 0)
            {

            }
            return (areThereEmbellishments, retObj);

        }


        private static List<Note> RemoveShortEmbelishments(List<Note> notes, List<List<Note>> embelishments)
        {
            var retObj = notes.Clone().OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();
            foreach (List<Note> embelishment in embelishments)
            {
                foreach (Note n in embelishment)
                {
                    RemoveNote(retObj, n);
                }
            }
            return retObj;
        }

        private static void RemoveNote(List<Note> notes, Note n)
        {
            var noteToRemove = notes.Where(x => x.StartSinceBeginningOfSongInTicks == n.StartSinceBeginningOfSongInTicks &&
                x.Voice == n.Voice && x.Pitch == n.Pitch && x.EndSinceBeginningOfSongInTicks == n.EndSinceBeginningOfSongInTicks).FirstOrDefault();
            if (noteToRemove != null)
                notes.Remove(noteToRemove);
        }

        /// <summary>
        /// Given one or more consecutive short notes in groups of 3 or less, it removes them and modifies the first long note after the group to
        /// start at the time of the first removed note
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        private static List<List<Note>> GetShortEmbellishments(List<Note> notes)
        {
            var retObj = new List<List<Note>>();
            for (int i = 0; i < notes.Count; i++)
            {
                var previousNote = i == 0 ? null : notes[i - 1];
                for (int j = 2; j < 5 && i + j <= notes.Count; j++)
                {
                    if (IsAnEmbelishment(previousNote, notes.GetRange(i, j)))
                    {
                        retObj.Add(notes.GetRange(i, j - 1));
                    }
                }
            }
            return retObj;
        }

        /// <summary>
        /// It checks if notes consists of 3 or less short notes followed by 1 long note (where short means less than a sixteenth and long a sixteen or longer)
        /// The note before the group of notes must be a long note (more than a sixteenth)
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        private static bool IsAnEmbelishment(Note? previousNote, List<Note> notes)
        {
            if (previousNote != null && previousNote.DurationInTicks < 21) return false;
            var orderedNotes = notes.OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();
            if (notes.Count > 4 || notes.Count < 2) return false;
            var lastNote = orderedNotes[orderedNotes.Count - 1];
            var firstNotes = orderedNotes.GetRange(0, orderedNotes.Count - 1);
            if (lastNote.DurationInTicks < 21 || firstNotes.Any(x => x.DurationInTicks > 15))
                return false;
            // The pitch step to the long note should not exceed an octave and a half
            if (Math.Abs(notes.Max(x => x.Pitch) - lastNote.Pitch) > 18 || Math.Abs(notes.Min(x => x.Pitch) - lastNote.Pitch) > 18)
                return false;
            return true;
        }

        /// <summary>
        /// Given a list of notes that contains trills, it replaces each trill by one note that has the pitch of the first note and the duration of the trill
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        private static List<Note> RemoveTrills(List<Note> notes, List<List<Note>> trills)
        {
            var retObj = notes.Clone().OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();
            foreach (List<Note> trill in trills)
            {
                var replacementNote = (Note)trill[0].Clone();
                replacementNote.EndSinceBeginningOfSongInTicks = trill[trill.Count - 1].EndSinceBeginningOfSongInTicks;
                foreach (Note n in trill)
                    RemoveNote(retObj, n);
                retObj.Add(replacementNote);
            }
            return retObj;
        }


        /// <summary>
        /// A trill, also known as a "shake", is a rapid alternation between an indicated note and the one above it.
        /// Returns all notes that belong to trills
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        private static List<List<Note>> GetTrills(List<Note> notes)
        {
            var retObj = new List<List<Note>>();
            var orderedNotes = PhraseDetection.RemoveHarmony(notes);
            for (int i = 0; i < orderedNotes.Count - 8; i++)
            {
                var j = 1;
                while (j + i < orderedNotes.Count && IsAtrill(orderedNotes.GetRange(i, j)))
                {
                    j++;
                }
                if (j > 8)
                {
                    retObj.Add(orderedNotes.GetRange(i, j));
                }
                i += j;
            }
            return retObj;
        }

        private static bool IsAtrill(List<Note> notes)
        {
            // Check they are all short
            if (notes.Where(x => x.DurationInTicks > 15).Count() > 0) return false;
            // Check there are only 2 different pitches
            if (notes.Select(x => x.Pitch).Distinct().Count() > 2) return false;
            // If the difference between the 2 notes is more than 3 semitones, it is not a trill
            if (notes.Select(x => x.Pitch).Max() - notes.Select(x => x.Pitch).Min() > 3) return false;
            // Check that goes up and down alternatively
            for (int j = 0; j < notes.Count() - 2; j++)
            {
                if ((notes[j + 1].Pitch - notes[j].Pitch) * (notes[j + 2].Pitch - notes[j + 1].Pitch) >= 0)
                    return false;
            }
            return true;
        }
    }
}
