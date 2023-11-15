using System;
using System.Diagnostics.CodeAnalysis;
#if NETSTANDARD2_0
using System.Runtime.Serialization;
#endif

namespace AdaskoTheBeAsT.WkHtmlToX.Exceptions
{
    [ExcludeFromCodeCoverage]
    [Serializable]
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    public class HtmlContentStreamTooLargeException
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
        : Exception
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

#if NETSTANDARD2_0
        protected HtmlContentStreamTooLargeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
