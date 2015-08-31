using System;

namespace NuDB.Exceptions
{
    public class InvalidLogFileException : Exception
    {
        public InvalidLogFileException() : base() { }
        public InvalidLogFileException(string message) : base(message) { }
    }
}
