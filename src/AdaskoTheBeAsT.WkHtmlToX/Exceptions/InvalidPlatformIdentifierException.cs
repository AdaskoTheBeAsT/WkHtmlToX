using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace AdaskoTheBeAsT.WkHtmlToX.Exceptions
{
    [ExcludeFromCodeCoverage]
    [Serializable]
    public class InvalidPlatformIdentifierException : Exception
    {
        public InvalidPlatformIdentifierException()
        {
        }

        public InvalidPlatformIdentifierException(string message)
            : base(message)
        {
        }

        public InvalidPlatformIdentifierException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected InvalidPlatformIdentifierException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
