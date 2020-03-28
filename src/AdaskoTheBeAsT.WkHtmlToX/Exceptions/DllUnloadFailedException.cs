using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace AdaskoTheBeAsT.WkHtmlToX.Exceptions
{
    [ExcludeFromCodeCoverage]
    [Serializable]
    public class DllUnloadFailedException : Exception
    {
        public DllUnloadFailedException()
        {
        }

        public DllUnloadFailedException(string message)
            : base(message)
        {
        }

        public DllUnloadFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected DllUnloadFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
