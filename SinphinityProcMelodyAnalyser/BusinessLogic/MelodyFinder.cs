using Sinphinity.Models;
using SinphinityModel.Helpers;
using SinphinityProcMelodyAnalyser.Models;
using SinphinitySysStore.Models;

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
                    var phraseInfo = PhraseDetection.GetPhraseBetweenEdges(cleanedVoiceNotes, phraseEdges[i], phraseEdges[i + 1], song.Id, voice, song.Bars);
                    retObj = AddPhraseToList(phraseInfo, retObj);
                }
            }
            retObj = AddEquivalences(retObj);
  
            return retObj;
        }
        private static List<ExtractedPhrase> AddEquivalences(List<ExtractedPhrase> extractePhrasesSoFar)
        {
            foreach (var p in extractePhrasesSoFar)
            {
                var maxDistance = PhraseDistance.MaxDistanceToBeConsideredEquivalent(p.PhraseType, p.AsString);
                p.Equivalences = extractePhrasesSoFar.Where(x => x.PhraseType == p.PhraseType && x.AsString!=p.AsString &&  PhraseDistance.GetDistance(p.PhraseType, p.AsString, x.AsString) < maxDistance)
                                .Select(y => y.AsString).ToList();
            }
            return extractePhrasesSoFar;
        }

        private static List<ExtractedPhrase> AddPhraseToList(PhraseInfo phraseInfo, List<ExtractedPhrase> extractePhrasesSoFar)
        {
            if (phraseInfo == null)
                return extractePhrasesSoFar;

            var metricPhrase = extractePhrasesSoFar.Where(x => x.PhraseType == PhraseTypeEnum.Metrics && x.AsString == phraseInfo.MetricsAsString).FirstOrDefault();
            if (metricPhrase == null)
            {
                metricPhrase = new ExtractedPhrase { AsString = phraseInfo.MetricsAsString, PhraseType = PhraseTypeEnum.Metrics, Occurrences = new List<PhraseLocation>() };
                extractePhrasesSoFar.Add(metricPhrase);
            }
            metricPhrase.Occurrences.Add(phraseInfo.Location);

            var pitchesPhrase = extractePhrasesSoFar.Where(x => x.PhraseType == PhraseTypeEnum.Pitches && x.AsString == phraseInfo.PitchesAsString).FirstOrDefault();
            if (pitchesPhrase == null)
            {
                pitchesPhrase = new ExtractedPhrase { AsString = phraseInfo.PitchesAsString, PhraseType = PhraseTypeEnum.Pitches, Occurrences = new List<PhraseLocation>() };
                extractePhrasesSoFar.Add(pitchesPhrase);
            }
            pitchesPhrase.Occurrences.Add(phraseInfo.Location);

            var phrase = extractePhrasesSoFar.Where(x => x.PhraseType == PhraseTypeEnum.Both && x.AsString == phraseInfo.PhraseAsString).FirstOrDefault();
            if (phrase == null)
            {
                phrase = new ExtractedPhrase { AsString = phraseInfo.PhraseAsString, PhraseType = PhraseTypeEnum.Both, Occurrences = new List<PhraseLocation>() };
                extractePhrasesSoFar.Add(phrase);
            }
            phrase.Occurrences.Add(phraseInfo.Location);

            if (phraseInfo.EmbellishedPhraseAsString != "/")
            {
                var embPhraseMetric = extractePhrasesSoFar.Where(x => x.PhraseType == PhraseTypeEnum.EmbelishedMetrics && x.AsString == phraseInfo.EmbellishedMetricsAsString).FirstOrDefault();
                if (embPhraseMetric == null)
                {
                    embPhraseMetric = new ExtractedPhrase { AsString = phraseInfo.EmbellishedMetricsAsString, PhraseType = PhraseTypeEnum.EmbelishedMetrics, Occurrences = new List<PhraseLocation>() };
                    extractePhrasesSoFar.Add(embPhraseMetric);
                }
                embPhraseMetric.Occurrences.Add(phraseInfo.Location);

                var embPhrasePitches = extractePhrasesSoFar.Where(x => x.PhraseType == PhraseTypeEnum.EmbelishedPitches && x.AsString == phraseInfo.EmbellishedPitchesAsString).FirstOrDefault();
                if (embPhrasePitches == null)
                {
                    embPhrasePitches = new ExtractedPhrase {
                        AsString = phraseInfo.EmbellishedPitchesAsString,
                        AsStringWithoutOrnaments= phraseInfo.PitchesAsString,
                        PhraseType = PhraseTypeEnum.EmbelishedPitches, 
                        Occurrences = new List<PhraseLocation>() };
                    extractePhrasesSoFar.Add(embPhrasePitches);
                }
                embPhrasePitches.Occurrences.Add(phraseInfo.Location);

                var embPhrase = extractePhrasesSoFar.Where(x => x.PhraseType == PhraseTypeEnum.EmbellishedBoth && x.AsString == phraseInfo.EmbellishedPhraseAsString).FirstOrDefault();
                if (embPhrase == null)
                {
                    embPhrase = new ExtractedPhrase {
                        AsString = phraseInfo.EmbellishedPhraseAsString,
                        AsStringWithoutOrnaments= phraseInfo.PhraseAsString,
                        PhraseType = PhraseTypeEnum.EmbellishedBoth,
                        Occurrences = new List<PhraseLocation>() };
                    extractePhrasesSoFar.Add(embPhrase);
                }
                embPhrase.Occurrences.Add(phraseInfo.Location);
            }            
            return extractePhrasesSoFar;
        }


        public static (int, int, int) GetBarBeatAndTickOfEdge(List<Bar> bars, long tick)
        {
            var barNo = bars.Where(b => b.TicksFromBeginningOfSong <= tick).Count();
            var beatLength = 4 * 96 / bars[barNo - 1].TimeSignature.Denominator;
            var beat = (int)(tick - bars[barNo - 1].TicksFromBeginningOfSong) / beatLength;
            var ticky = (int)(tick - bars[barNo - 1].TicksFromBeginningOfSong - beat * beatLength);
            return (barNo, beat, ticky);
        }
    }
}
