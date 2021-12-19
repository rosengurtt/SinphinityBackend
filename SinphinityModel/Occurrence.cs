namespace Sinphinity.Models
{
    /// <summary>
    /// Represent the location of the occurrence pattern at some point in a song
    /// </summary>
    public class Occurrence
    {
        public long SongId { get; set; }
        public byte Voice { get; set; }
        public long BarNumber { get; set; }
        public long Beat { get; set; }
        public long Tick { get; set; }
    }
}
