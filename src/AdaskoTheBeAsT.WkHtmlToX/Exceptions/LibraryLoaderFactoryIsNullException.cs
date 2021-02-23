using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace AdaskoTheBeAsT.WkHtmlToX.Exceptions
{
    [ExcludeFromCodeCoverage]
    [Serializable]
    public class LibraryLoaderFactoryIsNullException : Exception
    {
        public LibraryLoaderFactoryIsNullException()
        {
        }

        public LibraryLoaderFactoryIsNullException(string message)
            : base(message)
        {
        }

        public LibraryLoaderFactoryIsNullException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected LibraryLoaderFactoryIsNullException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
