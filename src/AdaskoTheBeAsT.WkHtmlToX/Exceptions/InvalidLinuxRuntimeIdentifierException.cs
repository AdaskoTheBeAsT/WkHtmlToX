using System;
using System.Diagnostics.CodeAnalysis;
#if NETSTANDARD2_0
using System.Runtime.Serialization;
#endif

namespace AdaskoTheBeAsT.WkHtmlToX.Exceptions;

[ExcludeFromCodeCoverage]
[Serializable]
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
public class InvalidLinuxRuntimeIdentifierException
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
    : Exception
{
    public InvalidLinuxRuntimeIdentifierException()
    {
    }

    public InvalidLinuxRuntimeIdentifierException(string message)
        : base(message)
    {
    }

    public InvalidLinuxRuntimeIdentifierException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

#if NETSTANDARD2_0
    protected InvalidLinuxRuntimeIdentifierException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
#endif
}
