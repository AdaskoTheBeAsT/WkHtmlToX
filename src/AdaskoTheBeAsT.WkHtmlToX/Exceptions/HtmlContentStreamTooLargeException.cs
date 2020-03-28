using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace AdaskoTheBeAsT.WkHtmlToX.Exceptions
{
    [ExcludeFromCodeCoverage]
    [Serializable]
    public class HtmlContentStreamTooLargeException : Exception
    {
        public HtmlContentStreamTooLargeException()
        {
        }

        public HtmlContentStreamTooLargeException(string message)
            : base(message)
        {
        }

        public HtmlContentStreamTooLargeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected HtmlContentStreamTooLargeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
