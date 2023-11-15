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
    public class DllNotLoadedException
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
        : Exception
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

#if NETSTANDARD2_0
        protected DllNotLoadedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
