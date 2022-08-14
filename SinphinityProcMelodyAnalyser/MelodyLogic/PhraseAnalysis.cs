using Sinphinity.Models;

namespace SinphinityProcMelodyAnalyser.MelodyLogic
{
    public static class PhraseAnalysis
    {

        public static List<ExtractedPhrase> FindAllPhrases(Song song, int songSimplification = 0)
        {
            throw new NotImplementedException();
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
          
            return edgesSoFar;
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
            if (followingEdge - candidateLocation < 144 && notesAfter < 6) return false;
            if (longestBefore > 0.9 * candidateDuration || longestAfter > 0.9 * candidateDuration) return false;
            return true;
        }

    }
}
