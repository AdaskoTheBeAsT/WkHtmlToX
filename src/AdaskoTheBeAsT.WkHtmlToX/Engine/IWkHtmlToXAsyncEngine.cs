using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Interop.Execution;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;

namespace AdaskoTheBeAsT.WkHtmlToX.Engine;

/// <summary>
/// Task-returning conversion and lifecycle API. The legacy engine interface remains
/// available for existing implementations.
/// </summary>
public interface IWkHtmlToXAsyncEngine : IWkHtmlToXEngine, IAsyncDisposable
{
    bool IsFaulted { get; }

    Exception? Fault { get; }

    int QueueDepth { get; }

    /// <summary>Cancels only the caller's initialization wait, not shared startup.</summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>Stops admission and joins the whole pipeline with the configured worker policy; cancellation only stops waiting.</summary>
    Task ShutdownAsync(CancellationToken cancellationToken = default);

    /// <summary>Stops admission and joins the whole pipeline. The first shutdown call selects the policy.</summary>
    Task ShutdownAsync(ExecutionShutdownMode shutdownMode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Snapshots built-in settings and byte arrays before returning. Input and output streams
    /// remain caller-owned and must not be accessed or disposed until this task finishes.
    /// Cancellation cannot interrupt an active native call.
    /// </summary>
    Task<ConversionResult> ConvertPdfAsync(
        IHtmlToPdfDocument document,
        Func<int, Stream> createStreamFunc,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Snapshots built-in settings before returning. The destination factory runs off the native
    /// thread after converter destruction. Its stream remains borrowed until this task finishes.
    /// </summary>
    Task<ConversionResult> ConvertImageAsync(
        IHtmlToImageDocument document,
        Func<int, Stream> createStreamFunc,
        CancellationToken cancellationToken = default);
}
