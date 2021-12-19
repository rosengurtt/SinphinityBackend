namespace SinphinitySysStore.Models
{
    /// <summary>
    /// Class used for the join table PatternsSongs
    /// </summary>
    public class PatternSong
    {
        public long Id { get; set; }
        public long PatternId { get; set; }
        public long SongId { get; set; }
    }
}
