﻿namespace SinphinitySysStore.Models
{
    public class SongAlreadyExistsException : Exception
    {
        public SongAlreadyExistsException()
        {
        }

        public SongAlreadyExistsException(string message)
            : base(message)
        {
        }

        public SongAlreadyExistsException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
