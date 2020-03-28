using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace AdaskoTheBeAsT.WkHtmlToX.Exceptions
{
    [ExcludeFromCodeCoverage]
    [Serializable]
    public class GetGlobalSettingsFailedException : Exception
    {
        public GetGlobalSettingsFailedException()
        {
        }

        public GetGlobalSettingsFailedException(string message)
            : base(message)
        {
        }

        public GetGlobalSettingsFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected GetGlobalSettingsFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
