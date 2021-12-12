namespace SinphinitySysStore.Models
{
    public class SongDoesntExistException : Exception
    {
        public SongDoesntExistException()
        {
        }

        public SongDoesntExistException(string message)
            : base(message)
        {
        }

        public SongDoesntExistException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
