using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace AdaskoTheBeAsT.WkHtmlToX.Exceptions
{
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class InvalidLinuxRuntimeIdentifierException : Exception
    {
        public InvalidLinuxRuntimeIdentifierException()
        {
        }

        public InvalidLinuxRuntimeIdentifierException(string message)
            : base(message)
        {
        }

        public InvalidLinuxRuntimeIdentifierException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected InvalidLinuxRuntimeIdentifierException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
