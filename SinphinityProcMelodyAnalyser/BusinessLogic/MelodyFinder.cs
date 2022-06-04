using Sinphinity.Models;
using SinphinityModel.Helpers;

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
        public static List<Dictionary<string, List<SongLocation>>> FindAllPhrases(Song song, int songSimplification=0)
        {
            var retObjPhrases = new Dictionary<string, List<SongLocation>>();
            var retObjEmbellishedPhrases = new Dictionary<string, List<SongLocation>>();
            var retObjPhrasesMetrics = new Dictionary<string, List<SongLocation>>();
            var retObjPhrasesPitches = new Dictionary<string, List<SongLocation>>();
            var retObjEmbellishedPhrasesMetrics = new Dictionary<string, List<SongLocation>>();
            var retObjEmbellishedPhrasesPitches = new Dictionary<string, List<SongLocation>>();
            var notes = song.SongSimplifications.Where(x => x.Version == songSimplification).FirstOrDefault()?.Notes;
            var preprocessedNotes = Preprocessor.RemoveDrumsAndChordsVoices(notes);
            preprocessedNotes = Preprocessor.RemoveBoringBassVoices(preprocessedNotes);
            preprocessedNotes = Preprocessor.SplitMultiVoiceTracks(preprocessedNotes);

            foreach (var voice in preprocessedNotes.Voices())
            {
                var voiceNotes = preprocessedNotes.Where(x => x.Voice == voice).OrderBy(y => y.StartSinceBeginningOfSongInTicks).ThenByDescending(z => z.Pitch).ToList();
                if (voiceNotes.Where(x => x.DurationInTicks <= 0).Any())
                {
                }
       

                var cleanedVoiceNotes = PhraseDetection.DiscretizeTiming(voiceNotes);
                if (cleanedVoiceNotes.Where(x => x.DurationInTicks <= 0).Any())
                {
                }

                cleanedVoiceNotes = PhraseDetection.RemoveHarmony(cleanedVoiceNotes);
                if (cleanedVoiceNotes.Where(x => x.DurationInTicks <= 0).Any())
                {
                }
                cleanedVoiceNotes = PhraseDetection.GetNotesWithSilencesRemoved(cleanedVoiceNotes);
                if (cleanedVoiceNotes.Where(x => x.DurationInTicks < 0).Any())
                {
                }
                // We remove harmony again because it could have been introduced in the previous step
                cleanedVoiceNotes = PhraseDetection.RemoveHarmony(cleanedVoiceNotes);
                if (cleanedVoiceNotes.Where(x => x.DurationInTicks <= 0).Any())
                {
                }
                var phraseEdges = PhraseDetection.GetPhrasesEdges(cleanedVoiceNotes, song.Bars).OrderBy(x => x).ToList().OrderBy(x => x).ToList();

                var sorets = new List<(int, int, int)>();
                foreach (var edgy in phraseEdges)
                {
                    var soret = GetBarBeatAndTickOfEdge(song.Bars, edgy);
                    sorets.Add(soret);
                }


                for (int i = 0; i < phraseEdges.Count - 1; i++)
                {
                    var phraseInfo = PhraseDetection.GetPhraseBetweenEdges(cleanedVoiceNotes, phraseEdges[i], phraseEdges[i + 1], song.Id, voice, song.Bars);
                   
                    if (phraseInfo != null)
                    {
                        if (phraseInfo.MetricsAsString.Split(",").Length > 40)
                        {
                            (var bar, var beat, var tick) = GetBarBeatAndTickOfEdge(song.Bars, phraseEdges[i]);
                            var instrument = cleanedVoiceNotes[0].Instrument;
                        }
                        if (phraseInfo.EmbellishedPitchesAsString == "2,-2,2,-2,2,-2,2,-2,2,-2,2,-2,2,-2,2,-2,2,-2,2,-2,2,0,-2,2,-2,2,-2,2,-2,2,-2")
                        {

                        }


                        if (!retObjPhrases.ContainsKey(phraseInfo.PhraseAsString))
                            retObjPhrases.Add(phraseInfo.PhraseAsString, new List<SongLocation>());
                        retObjPhrases[phraseInfo.PhraseAsString].Add(phraseInfo.Location);

                        if (!retObjPhrasesMetrics.ContainsKey(phraseInfo.MetricsAsString))
                            retObjPhrasesMetrics.Add(phraseInfo.MetricsAsString, new List<SongLocation>());
                        retObjPhrasesMetrics[phraseInfo.MetricsAsString].Add(phraseInfo.Location);

                        if (!retObjPhrasesPitches.ContainsKey(phraseInfo.PitchesAsString))
                            retObjPhrasesPitches.Add(phraseInfo.PitchesAsString, new List<SongLocation>());
                        retObjPhrasesPitches[phraseInfo.PitchesAsString].Add(phraseInfo.Location);

                        if (phraseInfo.EmbellishedPhraseAsString != "/")
                        {
                            // To build later the EmbellishedPhrase object from the asString version, we will need the related phrase without embellishments,
                            // so we add it to the key separated by the "|" symbol
                            var embellishedPhraseKey = $"{phraseInfo.EmbellishedPhraseAsString}|{phraseInfo.PhraseAsString}";
                            if (!retObjEmbellishedPhrases.ContainsKey(embellishedPhraseKey))
                                retObjEmbellishedPhrases.Add(embellishedPhraseKey, new List<SongLocation>());
                            retObjEmbellishedPhrases[embellishedPhraseKey].Add(phraseInfo.Location);

                            var embellishedPhrasesMetricsKey = $"{phraseInfo.EmbellishedMetricsAsString}|{phraseInfo.MetricsAsString}";
                            if (!retObjEmbellishedPhrasesMetrics.ContainsKey(embellishedPhrasesMetricsKey))
                                retObjEmbellishedPhrasesMetrics.Add(embellishedPhrasesMetricsKey, new List<SongLocation>());
                            retObjEmbellishedPhrasesMetrics[embellishedPhrasesMetricsKey].Add(phraseInfo.Location);

                            var embellishedPhrasesPitches = $"{phraseInfo.EmbellishedPitchesAsString}|{phraseInfo.PitchesAsString}";
                            if (!retObjEmbellishedPhrasesPitches.ContainsKey(embellishedPhrasesPitches))
                                retObjEmbellishedPhrasesPitches.Add(embellishedPhrasesPitches, new List<SongLocation>());
                            retObjEmbellishedPhrasesPitches[embellishedPhrasesPitches].Add(phraseInfo.Location);
                        }
                    }
                }
            }
            return new List<Dictionary<string, List<SongLocation>>>() { retObjPhrasesMetrics, retObjPhrasesPitches, retObjPhrases,
                retObjEmbellishedPhrasesMetrics, retObjEmbellishedPhrasesPitches, retObjEmbellishedPhrases };
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
