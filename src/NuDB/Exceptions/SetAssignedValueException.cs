using System;

namespace NuDB.Exceptions
{
    public class SetAssignedValueException : Exception
    {
        public SetAssignedValueException() : base() { }
        public SetAssignedValueException(string message) : base(message) { }
    }
}
