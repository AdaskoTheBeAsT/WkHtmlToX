using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace AdaskoTheBeAsT.WkHtmlToX.Exceptions
{
    [ExcludeFromCodeCoverage]
    [Serializable]
    public class GetObjectSettingsFailedException : Exception
    {
        public GetObjectSettingsFailedException()
        {
        }

        public GetObjectSettingsFailedException(string message)
            : base(message)
        {
        }

        public GetObjectSettingsFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected GetObjectSettingsFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
