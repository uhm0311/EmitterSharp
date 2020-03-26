using System;

namespace EventEmitterSharp
{
    public class SimpleEventException : Exception
    {
        public SimpleEventException(string message) : base(message) { }

        public SimpleEventException(string message, Exception innerException) : base(message, innerException) { }
    }
}
