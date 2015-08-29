using System;

namespace NuDB.Exceptions
{
    public class SetNotReadOnlyException : Exception
    {
        public SetNotReadOnlyException() : base() { }
        public SetNotReadOnlyException(string message) : base(message) { }
    }
}
