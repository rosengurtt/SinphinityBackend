using Sinphinity.Models;
using SinphinityModel.Helpers;

namespace SinphinityProcMidi.Helpers
{
    public static class PhraseConverter
    {

        public static List<Note> GetPhraseNotes( Phrase phrase, int instrument, byte startingPitch = 60)
        {

                    return phrase.AsSong.SongSimplifications[0].Notes;
      
        }


    }
}