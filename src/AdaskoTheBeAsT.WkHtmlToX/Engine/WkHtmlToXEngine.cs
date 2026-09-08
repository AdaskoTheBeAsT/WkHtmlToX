#pragma warning disable CS0618 // Intentional legacy compatibility implementation or regression coverage.
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Interop.Execution;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Loaders;
using AdaskoTheBeAsT.WkHtmlToX.WorkItems;

namespace AdaskoTheBeAsT.WkHtmlToX.Engine;

public sealed partial class WkHtmlToXEngine
    : IWkHtmlToXAsyncEngine
{
    private const string WorkerName = "WkHtmlToX Engine Worker";

    private readonly IExecutionWorker<WkHtmlToXSession> _worker;
    private readonly bool _ownsWorker;

    public WkHtmlToXEngine(WkHtmlToXConfiguration configuration)
        : this(
            CreateDefaultWorker(configuration),
            ownsWorker: true,
            configuration.RequestOptions)
    {
    }

    internal WkHtmlToXEngine(
        IExecutionWorker<WkHtmlToXSession> worker,
        bool ownsWorker = true,
        WkHtmlToXRequestOptions? requestOptions = null)
    {
        _worker = worker ?? throw new ArgumentNullException(nameof(worker));
        _ownsWorker = ownsWorker;
        _requestOptions = (requestOptions ?? new WkHtmlToXRequestOptions()).Snapshot();
        if (ownsWorker && worker is WkHtmlToXWorker nativeWorker)
        {
            nativeWorker.CoordinateShutdown(this);
        }
    }

    public bool IsFaulted => _worker.IsFaulted;

    public Exception? Fault => _worker.Fault;

    public int QueueDepth => _worker.QueueDepth;

    public void Initialize() => _worker.Initialize();

    public Task InitializeAsync(CancellationToken cancellationToken = default) =>
        _worker.InitializeAsync(cancellationToken);

    public Task<ConversionResult> ConvertPdfAsync(
        IHtmlToPdfDocument document,
        Func<int, Stream> createStreamFunc,
        CancellationToken cancellationToken = default) =>
        AdmitAsync(
            reserve => RequestSnapshot.Create(document, _requestOptions.MaxInputBytes, reserve),
            (snapshot, stream, token) => ExecuteConversionAsync(
                (session, requestToken) => session.PdfProcessor.ConvertWithResult(snapshot, stream, requestToken),
                token),
            createStreamFunc,
            cancellationToken);

    public Task<ConversionResult> ConvertImageAsync(
        IHtmlToImageDocument document,
        Func<int, Stream> createStreamFunc,
        CancellationToken cancellationToken = default) =>
        AdmitAsync(
            _ => RequestSnapshot.Create(document),
            (snapshot, stream, token) => ExecuteConversionAsync(
                (session, requestToken) => session.ImageProcessor.ConvertWithResult(snapshot, stream, requestToken),
                token),
            createStreamFunc,
            cancellationToken);

#pragma warning disable S1133 // Planned removal in the next major release; retained for migration.
    [Obsolete("Use ConvertPdfAsync or ConvertImageAsync instead.")]
#pragma warning restore S1133
    public void AddConvertWorkItem(
        ConvertWorkItemBase item,
        CancellationToken cancellationToken)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(item);
#else
        if (item is null)
        {
            throw new ArgumentNullException(nameof(item));
        }
#endif

        Task<bool> executionTask;
        try
        {
            executionTask = item switch
            {
                PdfConvertWorkItem pdf => ToLegacyResultAsync(ConvertPdfAsync(pdf.Document, pdf.StreamFunc, cancellationToken)),
                ImageConvertWorkItem image => ToLegacyResultAsync(ConvertImageAsync(image.Document, image.StreamFunc, cancellationToken)),
#pragma warning disable MA0025
                _ => Task.FromException<bool>(
                    new NotSupportedException($"Unsupported item type: {item.GetType().FullName}")),
#pragma warning restore MA0025
            };
        }
        catch (ObjectDisposedException)
        {
            item.TaskCompletionSource.TrySetException(new ObjectDisposedException(nameof(WkHtmlToXEngine)));
            throw;
        }
        catch (OperationCanceledException)
        {
            item.TaskCompletionSource.TrySetCanceled(cancellationToken);
            throw;
        }

        _ = executionTask.ContinueWith(
            ForwardResult,
            item,
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }

    internal static async Task<bool> ToLegacyResultAsync(Task<ConversionResult> task) =>
#pragma warning disable VSTHRD003 // Joins a conversion task owned by this engine's dedicated worker.
        (await task.ConfigureAwait(false)).ToLegacyResult();
#pragma warning restore VSTHRD003

    private static void ForwardResult(Task<bool> task, object? state)
    {
        var item = (ConvertWorkItemBase)state!;

        if (task.IsCanceled)
        {
            item.TaskCompletionSource.TrySetCanceled();
            return;
        }

        if (task.IsFaulted)
        {
            item.TaskCompletionSource.TrySetException(task.Exception.InnerExceptions);
            return;
        }

#pragma warning disable VSTHRD002 // Task is guaranteed complete inside a ContinueWith callback.
        item.TaskCompletionSource.TrySetResult(task.Result);
#pragma warning restore VSTHRD002
    }

    private static WkHtmlToXWorker CreateDefaultWorker(
        WkHtmlToXConfiguration configuration)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(configuration);
#else
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }
#endif

        var sessionFactory = new WkHtmlToXSessionFactory(
            configuration.Snapshot(),
            new LibraryLoaderFactory());

        var options = new ExecutionWorkerOptions(
            name: WorkerName,
            useStaThread: true);

        return new WkHtmlToXWorker(sessionFactory, options);
    }

    private async Task<ConversionResult> ExecuteConversionAsync(
        Func<WkHtmlToXSession, CancellationToken, ConversionResult> convert,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _worker.ExecuteAsync(
                (session, token) =>
                {
                    // This gate is the native-start boundary, serialized with shutdown.
                    if (IsPendingCanceled())
                    {
                        return new ConversionResult(ConversionFailureKind.Cancellation);
                    }

                    ConversionResult result;
                    try
                    {
                        result = convert.Invoke(session, token);
                    }
                    catch (ArgumentException exception)
                    {
                        return new ConversionResult(ConversionFailureKind.InvalidInput, exception: exception);
                    }

                    if (result.FailureKind == ConversionFailureKind.NativeRuntimeError)
                    {
                        throw new NativeConversionException(result);
                    }

                    if (result.FailureKind == ConversionFailureKind.Cancellation)
                    {
                        token.ThrowIfCancellationRequested();
                    }

                    return result;
                },
                new ExecutionRequestOptions(recycleSessionOnFailure: true),
                cancellationToken).ConfigureAwait(false);
            if (result.FailureKind == ConversionFailureKind.Cancellation)
            {
                throw new OperationCanceledException(new CancellationToken(canceled: true));
            }

            return result;
        }
        catch (NativeConversionException exception)
        {
            return exception.Result;
        }
    }

#pragma warning disable S3871 // Private transport signal, caught inside the public conversion boundary.
    private sealed class NativeConversionException(ConversionResult result)
        : Exception("Native conversion failed.", result.Exception)
    {
        internal ConversionResult Result { get; } = result;
    }
#pragma warning restore S3871
}

#pragma warning restore CS0618
