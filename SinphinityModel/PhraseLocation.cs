using System.Collections.Generic;
using System.Linq;

namespace Sinphinity.Models
{
    /// <summary>
    /// Provides all the information to locate a specific phrase in a song.
    /// Bar numbers start in 1, so if we have an arrange of bars called Bars, the bar of this location is Bars[BarNumber-1]
    /// </summary>
    public class PhraseLocation
    {
        public PhraseLocation() { }
        public PhraseLocation(long songId, byte voice, byte subVoice, long startTick, long endTick, byte instrument, int startingPitch,  List<Bar> bars) {

            BarNumber = bars.Where(b => b.TicksFromBeginningOfSong <= startTick).Count();
            var beatLength = 4 * 96 / bars[BarNumber - 1].TimeSignature.Denominator;
            Beat = 1 + (int)(startTick - bars[BarNumber - 1].TicksFromBeginningOfSong) / beatLength;
            SongId = songId;
            Voice = voice;
            SubVoice = subVoice;
            StartTick = startTick;
            EndTick = endTick;
            StartingPitch = startingPitch;
            Instrument = instrument;
        }

        public long SongId { get; set; }
        public byte Voice { get; set; }
        public byte SubVoice { get; set; }
        public byte Instrument { get; set; }
        public int BarNumber { get; set; }
        public int Beat { get; set; }
        public long StartTick { get; set; }
        public long EndTick { get; set; }
        public int StartingPitch { get; set; }
    }
}
