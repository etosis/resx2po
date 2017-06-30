using System;
using System.Runtime.Serialization;

namespace etosis.resx2po
{
    [Serializable]
    internal class UsageException : Exception
    {
        public UsageException()
        {
        }

        public UsageException(string message) : base(message)
        {
        }

        public UsageException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UsageException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}