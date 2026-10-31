using System;

namespace Lift.Data
{
    /// <summary>A database problem, with a message that is safe to show to the user.</summary>
    public sealed class DataAccessException : Exception
    {
        public DataAccessException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
