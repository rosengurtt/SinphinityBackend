namespace SinphinityModel.Pattern
{
    /// <summary>
    /// Represents an association of a pattern with a song
    /// Basically is saying pattern X is present in song Y
    /// </summary>
    public class PatternSong
    {
        public string Id { get; set; }

        public string PatternId { get; set; }

        public string PatternAsString { get; set; }
        public string SongInfoId { get; set; }

        public string SongName { get; set; }
        public string StyleName { get; set; }
        public string BandName { get; set; }
    }
}
