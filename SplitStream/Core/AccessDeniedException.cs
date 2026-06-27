using System;

namespace SplitStream
{
    public class AccessDeniedException : Exception
    {
        public AccessDeniedException() : base("Access to the destination path is denied.")
        {
        }

        public AccessDeniedException(string message) : base(message)
        {
        }

        public AccessDeniedException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
