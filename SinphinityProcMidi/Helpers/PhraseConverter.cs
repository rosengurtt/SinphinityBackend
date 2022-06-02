using Sinphinity.Models;
using SinphinityModel.Helpers;

namespace SinphinityProcMidi.Helpers
{
    public static class PhraseConverter
    {

        public static List<Note> GetPhraseNotes(PhraseTypeEnum phraseType, string asString, int instrument, byte startingPitch = 60)
        {
            var metricsAsString = GetMetricsAsString(phraseType, asString);
            var pitchesAsString = GetPitchesAsString(phraseType, asString);
            return GetNotes(metricsAsString, pitchesAsString, instrument, startingPitch);
        }


        private static string GetMetricsAsString(PhraseTypeEnum phraseType, string asString)
        {

            switch (phraseType)
            {
                case PhraseTypeEnum.Metrics:
                case PhraseTypeEnum.EmbelishedMetrics:
                    return asString;
                case PhraseTypeEnum.Pitches:
                case PhraseTypeEnum.EmbelishedPitches:
                    var phrasePitches = new PhrasePitches(asString);
                    string retObj = "";
                    for (var i = 0; i < phrasePitches.Items.Count; i++)
                    {
                        retObj += "96";
                        if (i < phrasePitches.Items.Count - 1)
                            retObj += ",";
                    }
                    return retObj;
                case PhraseTypeEnum.Both:
                case PhraseTypeEnum.EmbellishedBoth:
                    var parts = asString.Split("/");
                    return parts[0];
            }
            return null;
        }
        private static string GetPitchesAsString(PhraseTypeEnum phraseType, string asString)
        {

            switch (phraseType)
            {
                case PhraseTypeEnum.Metrics:
                case PhraseTypeEnum.EmbelishedMetrics:
                    var phrasePitches = new PhrasePitches(asString);
                    string retObj = "";
                    for (var i = 0; i < phrasePitches.Items.Count - 1; i++)
                    {
                        retObj += "0";
                        if (i < phrasePitches.Items.Count - 2)
                            retObj += ",";
                    }
                    return retObj;
                case PhraseTypeEnum.Pitches:
                case PhraseTypeEnum.EmbelishedPitches:
                    return asString;
                case PhraseTypeEnum.Both:
                case PhraseTypeEnum.EmbellishedBoth:
                    var parts = asString.Split("/");
                    return parts[1];
            }
            return null;
        }


        private static List<Note> GetNotes(string metricsAsString, string pitchesAsString, int instrument = 0, byte startingPitch = 60)
        {
            var retObj = new List<Note>();

            var phraseMetrics = new PhraseMetrics(metricsAsString);
            var phrasePitches = new PhrasePitches(pitchesAsString);
            long ticksFromStart = 0;
            byte currentPitch = startingPitch;
            retObj.Add(new Note
            {
                Pitch = startingPitch,
                StartSinceBeginningOfSongInTicks = 0,
                EndSinceBeginningOfSongInTicks = phraseMetrics.Items[0],
                Instrument = (byte)instrument,
                Volume = 90
            });
            for (int i = 0; i < phrasePitches.Items.Count; i++)
            {
                currentPitch += (byte)phrasePitches.Items[i];
                retObj.Add(new Note
                {
                    StartSinceBeginningOfSongInTicks = ticksFromStart + phraseMetrics.Items[i],
                    EndSinceBeginningOfSongInTicks = i + 1 < phrasePitches.Items.Count ?
                        ticksFromStart + phraseMetrics.Items[i] + phraseMetrics.Items[i + 1] :
                        ticksFromStart + phraseMetrics.Items[i] + 96,
                    Pitch = currentPitch,
                    Instrument = (byte)instrument,
                    Volume = 90
                });
                ticksFromStart += phraseMetrics.Items[i];
            }
            return retObj;
        }
    }
}