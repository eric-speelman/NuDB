using System;

namespace NuDB.Exceptions
{
    public class SetTypeMisMatchException : Exception
    {
        public SetTypeMisMatchException() : base() { }
        public SetTypeMisMatchException(string message) : base(message) { }
    }
}
