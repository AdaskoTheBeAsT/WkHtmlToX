using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace AdaskoTheBeAsT.WkHtmlToX.Exceptions
{
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class DllNotLoadedException : Exception
    {
        public DllNotLoadedException()
        {
        }

        public DllNotLoadedException(string message)
            : base(message)
        {
        }

        public DllNotLoadedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected DllNotLoadedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
