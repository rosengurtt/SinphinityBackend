using Sinphinity.Models;
using SinphinityModel.Helpers;
using System.Linq;

namespace SinphinityProcMidi.Midi
{
    public static partial class MidiUtilities
    {
        public static SongSimplification GetSimplificationZeroOfSong(string base64encodedMidiFile)
        {

            var notesObj = GetNotesOfMidiFile(base64encodedMidiFile);

            var retObj = new SongSimplification()
            {
                Notes = notesObj,
                Version = 0,
                NumberOfVoices = notesObj.Voices().Count()
            };
            return retObj;
        }
    }
}
