using System;
using System.Runtime.Serialization;

namespace Green
{
    [Serializable]
    internal class ReaderUnexpectedEof : ReaderException
    {
        public ReaderUnexpectedEof()
        {
        }

        public ReaderUnexpectedEof(string message) : base(message)
        {
        }

        public ReaderUnexpectedEof(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ReaderUnexpectedEof(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
