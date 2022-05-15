using System.Collections.Generic;
using System.Linq;

namespace Sinphinity.Models
{
    /// <summary>
    /// Provides all the information to locate a specific place in a song.
    /// Bar numbers start in 1, so if we have an arrange of bars called Bars, the bar of this location is Bars[BarNumber-1]
    /// </summary>
    public class SongLocation
    {
        public SongLocation() { }
        public SongLocation(long songId, byte voice, long startTick, long endTick, List<Bar> bars) {

            BarNumber = bars.Where(b => b.TicksFromBeginningOfSong <= startTick).Count();
            var beatLength = 4 * 96 / bars[BarNumber - 1].TimeSignature.Denominator;
            Beat = (int)(startTick - bars[BarNumber - 1].TicksFromBeginningOfSong) / beatLength;
            SongId = songId;
            Voice = voice;
            StartTick = startTick;
            EndTick = endTick;
        }

        public long SongId { get; set; }
        public byte Voice { get; set; }
        public int BarNumber { get; set; }
        public int Beat { get; set; }
        public long StartTick { get; set; }
        public long EndTick { get; set; }
    }
}
