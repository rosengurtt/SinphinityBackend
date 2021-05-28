namespace Sinphinity.Models
{
    public class PitchBendItem
    {
        public long TicksSinceBeginningOfSong { get; set; }
        public ushort Pitch { get; set; }

        public PitchBendItem Clone()
        {
            return new PitchBendItem
            {
                TicksSinceBeginningOfSong = this.TicksSinceBeginningOfSong,
                Pitch = this.Pitch
            };
        }
    }
}
