using System;
using System.Diagnostics.CodeAnalysis;
#if NETSTANDARD2_0
using System.Runtime.Serialization;
#endif

namespace AdaskoTheBeAsT.WkHtmlToX.Exceptions;

[ExcludeFromCodeCoverage]
[Serializable]
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
public class InvalidPlatformIdentifierException
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
    : Exception
{
    public InvalidPlatformIdentifierException()
    {
    }

    public InvalidPlatformIdentifierException(string message)
        : base(message)
    {
    }

    public InvalidPlatformIdentifierException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

#if NETSTANDARD2_0
    protected InvalidPlatformIdentifierException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
#endif
}
