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
    public class DllUnloadFailedException
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
        : Exception
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

#if NETSTANDARD2_0
        protected DllUnloadFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
