using System;
using System.Runtime.Serialization;

namespace Green
{
    [Serializable]
    class ReaderException : Exception
    {
        public ReaderException()
        {
        }

        public ReaderException(string message) : base(message)
        {
        }

        public ReaderException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ReaderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}