using Sinphinity.Models;

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
        public Song Song { get; set; }
        public Style Style { get; set; }
        public Band Band { get; set; }

    }
}
