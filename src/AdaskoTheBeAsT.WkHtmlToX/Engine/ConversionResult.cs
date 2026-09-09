using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

namespace AdaskoTheBeAsT.WkHtmlToX.Engine;

/// <summary>
/// Outcome after native converter destruction. Warning text and exceptions may
/// contain application data and should not be logged without redaction.
/// </summary>
public sealed class ConversionResult
{
    internal ConversionResult(
        ConversionFailureKind failureKind,
        int httpErrorCode = 0,
        Exception? exception = null,
        Exception? callbackException = null,
        IReadOnlyList<string>? warnings = null)
    {
        FailureKind = failureKind;
        HttpErrorCode = httpErrorCode;
        Exception = exception;
        CallbackException = callbackException;
        Warnings = warnings ?? Array.Empty<string>();
    }

    public bool Success => FailureKind == ConversionFailureKind.None;

    public ConversionFailureKind FailureKind { get; }

    public int HttpErrorCode { get; }

    public Exception? Exception { get; }

    public Exception? CallbackException { get; }

    /// <summary>Gets at most 32 native warnings, each limited to 1024 characters.</summary>
    public IReadOnlyList<string> Warnings { get; }

    internal bool ToLegacyResult()
    {
        var exception = Exception ?? CallbackException;
        if (exception is not null)
        {
            ExceptionDispatchInfo.Capture(exception).Throw();
        }

        return Success;
    }
}
