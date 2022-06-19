using Sinphinity.Models;
using SinphinityModel.Helpers;

namespace SinphinityProcMidi.Helpers
{
    public static class PhraseConverter
    {

        public static List<Note> GetPhraseNotes(PhraseTypeEnum phraseType, string asString, int instrument, byte startingPitch = 60)
        {
            switch (phraseType)
            {
                case PhraseTypeEnum.Metrics:
                case PhraseTypeEnum.EmbelishedMetrics:
                    var phraseMetrics = new PhraseMetrics(asString);
                    return phraseMetrics.AsSong.SongSimplifications[0].Notes;
                case PhraseTypeEnum.Pitches:
                case PhraseTypeEnum.EmbelishedPitches:
                    var phrasePitches = new PhrasePitches(asString);
                    return phrasePitches.AsSong.SongSimplifications[0].Notes;
                case PhraseTypeEnum.Both:
                case PhraseTypeEnum.EmbellishedBoth:
                    var parts = asString.Split("/");
                    var pm = new PhraseMetrics(parts[0]);
                    var pp = new PhrasePitches(parts[1]);
                    var phrase = new Phrase(pm, pp);
                    return phrase.AsSong.SongSimplifications[0].Notes;
            }
            throw new Exception("Que mierda me mandaron?");
        }


    }
}