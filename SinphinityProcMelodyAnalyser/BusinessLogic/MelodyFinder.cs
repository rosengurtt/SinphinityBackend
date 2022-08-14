using Sinphinity.Models;
using SinphinityModel.Helpers;
using SinphinityProcMelodyAnalyser.Models;

namespace SinphinityProcMelodyAnalyser.BusinessLogic
{
    public static class MelodyFinder
    {
        /// <summary>
        /// Returns 3 dictionaries:
        /// - the first has the phrases metrics and their locations (key is phrase metrics as string)
        /// - the second has the phrases pitches and their locations (key is phrase pitches as string0
        /// - the third has the phrases and their locations (key is phrase metric as string plus "/" plus phrase pitches as string
        /// </summary>
        /// <param name="song"></param>
        /// <param name="songSimplification"></param>
        /// <returns></returns>
        public static List<ExtractedPhrase> FindAllPhrases(Song song, int songSimplification = 0)
        {
            var retObj = new List<ExtractedPhrase>();


            var notes = song.SongSimplifications.Where(x => x.Version == songSimplification).FirstOrDefault()?.Notes;
            var preprocessedNotes = Preprocessor.RemoveDrumsAndChordsVoices(notes);
            preprocessedNotes = Preprocessor.RemoveBoringBassVoices(preprocessedNotes);
            preprocessedNotes = Preprocessor.SplitMultiVoiceTracks(preprocessedNotes);

            foreach (var voice in preprocessedNotes.Voices())
            {
                var voiceNotes = preprocessedNotes.Where(x => x.Voice == voice).OrderBy(y => y.StartSinceBeginningOfSongInTicks).ThenByDescending(z => z.Pitch).ToList();

                var cleanedVoiceNotes = PhraseDetection.DiscretizeTiming(voiceNotes);
                cleanedVoiceNotes = PhraseDetection.RemoveHarmony(cleanedVoiceNotes);
                cleanedVoiceNotes = PhraseDetection.GetNotesWithSilencesRemoved(cleanedVoiceNotes);
                // We remove harmony again because it could have been introduced in the previous step
                cleanedVoiceNotes = PhraseDetection.RemoveHarmony(cleanedVoiceNotes);

                var phraseEdges = PhraseDetection.GetPhrasesEdges(cleanedVoiceNotes, song.Bars).OrderBy(x => x).ToList().OrderBy(x => x).ToList();

                for (int i = 0; i < phraseEdges.Count - 1; i++)
                {
                    (var phrase, var location) = PhraseDetection.GetPhraseBetweenEdges(cleanedVoiceNotes, phraseEdges[i], phraseEdges[i + 1], song.Id, voice, song.Bars);
                    retObj = AddPhraseToList(phrase, location, retObj);
                }
            }
            retObj = AddEquivalences(retObj);
  
            return retObj;
        }
        private static List<ExtractedPhrase> AddEquivalences(List<ExtractedPhrase> extractePhrasesSoFar)
        {
            var maxPitchDistance = 0.4;
            var maxMetricsDistance = 0.4;
            foreach (var p in extractePhrasesSoFar)
            {
                p.Equivalences = extractePhrasesSoFar.Where(x => (PhraseDistance.GetMetricDistance(p.Phrase, x.Phrase) < maxMetricsDistance) && 
                    (PhraseDistance.GetPitchDistance( p.Phrase, x.Phrase) < maxPitchDistance))
                                .Select(y => $"{y.Phrase.MetricsAsString}/{y.Phrase.PitchesAsString}").ToList();
            }
            return extractePhrasesSoFar;
        }

        private static List<ExtractedPhrase> AddPhraseToList(Phrase phrase, PhraseLocation location, List<ExtractedPhrase> extractePhrasesSoFar)
        {
            if (phrase == null)
                return extractePhrasesSoFar;      

            var p = extractePhrasesSoFar.Where(x => x.Phrase.MetricsAsString== phrase.MetricsAsString && x.Phrase.PitchesAsString==phrase.PitchesAsString).FirstOrDefault();
            if (phrase == null)
            {
                p = new ExtractedPhrase { Phrase=phrase, Occurrences = new List<PhraseLocation>() };
                extractePhrasesSoFar.Add(p);
            }
            p.Occurrences.Add(location);

            return extractePhrasesSoFar;
        }


    }
}
