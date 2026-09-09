using System;

namespace AdaskoTheBeAsT.WkHtmlToX.Engine;

#pragma warning disable S3871 // Internal stage marker, unwrapped by ProcessorBase before returning to callers.
internal sealed class OutputWriteException(Exception innerException)
    : Exception("The destination stream operation failed.", innerException);
#pragma warning restore S3871
