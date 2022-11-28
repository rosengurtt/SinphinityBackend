using Sinphinity.Models;
using SinphinityModel.Helpers;

namespace SinphinityProcMelodyAnalyser.MelodyLogic
{
    public static class PhraseAnalysis
    {

        public static List<ExtractedPhrase> FindAllPhrases(List<Note> originalNotes, List<Note> preprocessedNotes, List<Bar> bars, long songId)
        {
            var retObj = new List<ExtractedPhrase>();

            foreach (var voice in preprocessedNotes.Voices())
            {
                foreach (var subVoice in new List<byte> { 0, 1 })
                {
                    var voiceNotes = preprocessedNotes.Where(x => x.Voice == voice && x.SubVoice == subVoice).OrderBy(y => y.StartSinceBeginningOfSongInTicks).ToList();
                    var phrasesEdges = GetPhrasesEdges(voiceNotes, bars);
                    for (int i = 0; i < phrasesEdges.Count - 1; i++)
                    {
                        (var phrase, var location) = GetPhraseBetweenEdges(originalNotes, voiceNotes, phrasesEdges[i], phrasesEdges[i + 1], songId, voice, subVoice, bars);
                        retObj = AddPhraseToList(phrase, location, retObj);
                    }
                }
            }
            retObj = AddEquivalences(retObj);
            return retObj;
        }
        private static List<ExtractedPhrase> AddPhraseToList(Phrase phrase, PhraseLocation location, List<ExtractedPhrase> extractePhrasesSoFar)
        {
            if (phrase == null)
                return extractePhrasesSoFar;

            var p = extractePhrasesSoFar.Where(x => AreMetricsEssentiallyTheSame(x.Phrase.MetricsAsString, phrase.MetricsAsString) && x.Phrase.PitchesAsString == phrase.PitchesAsString).FirstOrDefault();
            if (p == null)
            {
                p = new ExtractedPhrase { Phrase = phrase, Occurrences = new List<PhraseLocation>() };
                extractePhrasesSoFar.Add(p);
            }
            p.Occurrences.Add(location);

            return extractePhrasesSoFar;
        }
        /// <summary>
        /// When we compare 2 prhases to decide if they are the same phrase, we must have some tolerance regarding the timing
        /// For example if we have the metrics (95,97,48,47), this is essentially the same as (96,96,48,48)
        /// </summary>
        /// <param name="metrics1"></param>
        /// <param name="metrics2"></param>
        /// <returns></returns>
        private static Boolean AreMetricsEssentiallyTheSame(string metrics1, string metrics2)
        {
            var times1 = metrics1.ExpandPattern().Split(",").Select(x => int.Parse(x)).ToList();
            var times2 = metrics2.ExpandPattern().Split(",").Select(x => int.Parse(x)).ToList();
            if (times1.Count != times2.Count) return false;
            // if the total duration differs more than 10% we consider them different
            if (Math.Abs(times1.Sum() - times2.Sum()) > (times1.Sum() + times2.Sum()) / 20)
                return false;
            for (int i = 0; i < times1.Count; i++)
            {
                //  if the difference is more than 50% in any of the notes duration we consider the phrases different
                if (Math.Abs(times1[i] - times2[i]) > (times1[i] + times2[i]) / 4)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 2 different phrases may be actually the same phrase but played in different parts of a scale, so the relative pitches will not
        /// match exactly. For example C3E3F3 has relative pitches 4,1, while D3F3G3 has relative pitches 3,2, but they are essentially
        /// the same thing
        /// We calculate the distance in pitches and the distance in metrics, and the sum of th2 2 has to be less than 
        /// maxPitchDistance + maxMetricsDistance
        /// </summary>
        /// <param name="extractePhrasesSoFar"></param>
        /// <returns></returns>
        public static List<ExtractedPhrase> AddEquivalences(List<ExtractedPhrase> extractePhrasesSoFar)
        {
            var maxPitchDistance = 0.4;
            var maxMetricsDistance = 0.4;
            foreach (var p in extractePhrasesSoFar)
            {
                p.Equivalences = extractePhrasesSoFar.Where(x =>
                    (PhraseDistance.GetMetricDistance(p.Phrase, x.Phrase) + PhraseDistance.GetPitchDistance(p.Phrase, x.Phrase)) <= (maxMetricsDistance + maxPitchDistance) &&
                    p != x)
                    .Select(y => $"{y.Phrase.MetricsAsString}/{y.Phrase.PitchesAsString}").ToList();
            }
            return extractePhrasesSoFar;
        }
        public static (Phrase, PhraseLocation) GetPhraseBetweenEdges(List<Note> originalNotes, List<Note> notes, long start, long end, long songId, byte voice, byte subVoice, List<Bar> bars)
        {
            var phraseNotes = notes
                .Where(x => x.StartSinceBeginningOfSongInTicks >= start && x.StartSinceBeginningOfSongInTicks < end)
                .OrderBy(y => y.StartSinceBeginningOfSongInTicks)
                .ToList();
            if (phraseNotes.Count < 2)
                return (null, null);

            var location = new PhraseLocation(songId, voice, subVoice, start, end, phraseNotes[0].Instrument, phraseNotes[0].Pitch, bars);
            var lastNote = phraseNotes.OrderByDescending(x => x.StartSinceBeginningOfSongInTicks).FirstOrDefault();
            var lastNoteOriginalDuration = originalNotes.Where(x => x.Guid == lastNote.Guid).FirstOrDefault().DurationInTicks;
            lastNote.EndSinceBeginningOfSongInTicks = lastNote.StartSinceBeginningOfSongInTicks + lastNoteOriginalDuration;
            var phrase = new Phrase(phraseNotes);
            var skeleton = PhraseSkeleton.GetSkeleton(phrase);
            phrase.SkeletonMetricsAsString = skeleton.MetricsAsString != phrase.MetricsAsString ? skeleton.MetricsAsString : "";
            phrase.SkeletonPitchesAsString = skeleton.MetricsAsString != phrase.MetricsAsString ? skeleton.PitchesAsString : "";

            return (phrase, location);

        }
        /// <summary>
        /// Analyzes the metric of the song to find the points where phrases start and end
        /// Returns a list with the locations represented as the number of ticks since the beginning of the song 
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        public static List<long> GetPhrasesEdges(List<Note> notes, List<Bar> bars)
        {
            var edgesSoFar = new HashSet<long>();
            if (notes.Count == 0)
                return edgesSoFar.ToList();
            edgesSoFar.Add(notes[0].StartSinceBeginningOfSongInTicks);
            edgesSoFar.Add(notes[notes.Count - 1].EndSinceBeginningOfSongInTicks);

            edgesSoFar = GetEdgesOfSilencesAndLongNotes(notes, bars, edgesSoFar);
            edgesSoFar = ChangeOfPaceDetection.GetEdgesBetweenChangesInPacing(notes, edgesSoFar);
            edgesSoFar = IntervalJumpDetection.GetEdgesBetweenGroupsOfNotesWithSmallSteps(notes, edgesSoFar);
            edgesSoFar = PatternDetection.GetRepeatingPatterns(notes, edgesSoFar);

            return edgesSoFar.ToList().OrderBy(x => x).ToList();
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


        public static bool WillNewEdgeCreatePhraseTooShort(List<Note> notes, HashSet<long> edgesSoFar, long newEdge)
        {
            var endOfLastNote = notes.Max(x => x.EndSinceBeginningOfSongInTicks);
            var edgeBefore = edgesSoFar.Where(x => x < newEdge).Count() > 0 ? edgesSoFar.Where(x => x < newEdge).Max() : 0;
            var edgeAfter = edgesSoFar.Where(x => x > newEdge).Count() > 0 ? edgesSoFar.Where(x => x > newEdge).Min() : endOfLastNote;

            var notesBefore = notes.Where(x => x.StartSinceBeginningOfSongInTicks >= edgeBefore && x.StartSinceBeginningOfSongInTicks < newEdge);
            var notesAfter = notes.Where(x => x.StartSinceBeginningOfSongInTicks >= newEdge && x.StartSinceBeginningOfSongInTicks < edgeAfter);

            // if we would have too few notes and a short time don't add edge
            if ((newEdge - edgeBefore) * notesBefore.Count() < 384 * 4)
                return true;
            if ((edgeAfter - newEdge) * notesAfter.Count() < 384 * 4)
                return true;


            // If we are going to break a group of less than 12 notes, leave it
            var numberOfNotesBetweenEdges = notes.Where(x => x.StartSinceBeginningOfSongInTicks >= edgeBefore &&
                                                        x.EndSinceBeginningOfSongInTicks <= edgeAfter)
                                                   .Count();
            if (numberOfNotesBetweenEdges < 12)
                return true;

            // If the phrase we are going to break is less than 384 ticks, leave it
            var musicStart = notes.Where(x => x.StartSinceBeginningOfSongInTicks >= edgeBefore).OrderBy(x => x.StartSinceBeginningOfSongInTicks).First();
            var musicEnd = notes.Where(x => x.StartSinceBeginningOfSongInTicks < edgeAfter).OrderByDescending(x => x.StartSinceBeginningOfSongInTicks).First();
            if (musicEnd.StartSinceBeginningOfSongInTicks - musicStart.StartSinceBeginningOfSongInTicks < 384)
                return true;



            return false;
        }

    }
}
