namespace Sinphinity.Models
{
    public class TempoChange
    {
        /// <summary>
        /// This is the standard way of handling tempos in Midi files
        /// </summary>
        public long MicrosecondsPerQuarterNote { get; set; }

        /// <summary>
        /// Place in the song where the tempo changes
        /// </summary>
        public long TicksSinceBeginningOfSong { get; set; }
    }
}
