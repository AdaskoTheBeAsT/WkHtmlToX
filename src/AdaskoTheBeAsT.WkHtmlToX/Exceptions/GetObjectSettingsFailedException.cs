using System;
using System.Diagnostics.CodeAnalysis;
#if NETSTANDARD2_0
using System.Runtime.Serialization;
#endif

namespace AdaskoTheBeAsT.WkHtmlToX.Exceptions;

[ExcludeFromCodeCoverage]
[Serializable]
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
public class GetObjectSettingsFailedException
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
    : Exception
{
    public GetObjectSettingsFailedException()
    {
    }

    public GetObjectSettingsFailedException(string message)
        : base(message)
    {
    }

    public GetObjectSettingsFailedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

#if NETSTANDARD2_0
    protected GetObjectSettingsFailedException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
#endif
}
