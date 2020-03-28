using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace AdaskoTheBeAsT.WkHtmlToX.Exceptions
{
    [ExcludeFromCodeCoverage]
    [Serializable]
    public class HtmlContentEmptyException : Exception
    {
        public HtmlContentEmptyException()
        {
        }

        public HtmlContentEmptyException(string message)
            : base(message)
        {
        }

        public HtmlContentEmptyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected HtmlContentEmptyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
