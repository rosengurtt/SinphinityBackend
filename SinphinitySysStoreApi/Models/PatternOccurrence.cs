namespace SinphinitySysStore.Models
{
    public class PatternOccurrence
    {
        public long Id { get; set; }
        public long SongId { get; set; }
        public long PatternId { get; set; }
        public byte Voice { get; set; }
        public long BarNumber { get; set; }
        public long Beat { get; set; }
        public long Tick { get; set; }
    }
}
