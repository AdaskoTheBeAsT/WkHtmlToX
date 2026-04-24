using System;
using System.Diagnostics.CodeAnalysis;
#if !NET8_0_OR_GREATER
using System.Runtime.Serialization;
#endif

namespace AdaskoTheBeAsT.WkHtmlToX.Exceptions;

[ExcludeFromCodeCoverage]
[Serializable]
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
public class GetGlobalSettingsFailedException
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
    : Exception
{
    public GetGlobalSettingsFailedException()
    {
    }

    public GetGlobalSettingsFailedException(string message)
        : base(message)
    {
    }

    public GetGlobalSettingsFailedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

#if !NET8_0_OR_GREATER
    protected GetGlobalSettingsFailedException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
#endif
}
