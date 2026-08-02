using System;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Interop.Execution;
using AdaskoTheBeAsT.WkHtmlToX.Loaders;
using AdaskoTheBeAsT.WkHtmlToX.WorkItems;

namespace AdaskoTheBeAsT.WkHtmlToX.Engine;

public sealed class WkHtmlToXEngine
    : IWkHtmlToXEngine
{
    private const string WorkerName = "WkHtmlToX Engine Worker";

    private readonly IExecutionWorker<WkHtmlToXSession> _worker;
    private readonly bool _ownsWorker;

    public WkHtmlToXEngine(WkHtmlToXConfiguration configuration)
        : this(
            CreateDefaultWorker(configuration),
            ownsWorker: true)
    {
    }

    internal WkHtmlToXEngine(
        IExecutionWorker<WkHtmlToXSession> worker,
        bool ownsWorker = true)
    {
        _worker = worker ?? throw new ArgumentNullException(nameof(worker));
        _ownsWorker = ownsWorker;
    }

    public void Initialize() => _worker.Initialize();

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

        var options = new ExecutionRequestOptions(recycleSessionOnFailure: true);

        Task<bool> executionTask;
        try
        {
            executionTask = item switch
            {
                PdfConvertWorkItem pdf => _worker.ExecuteAsync(
                    (session, _) => session.PdfProcessor.Convert(pdf.Document, pdf.StreamFunc),
                    options,
                    cancellationToken),
                ImageConvertWorkItem image => _worker.ExecuteAsync(
                    (session, _) => session.ImageProcessor.Convert(image.Document, image.StreamFunc),
                    options,
                    cancellationToken),
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

    public void Dispose()
    {
        if (_ownsWorker)
        {
#pragma warning disable IDISP007 // Don't dispose injected. Worker is owned when constructed via the public ctor.
            _worker.Dispose();
#pragma warning restore IDISP007
        }
    }

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

    private static ExecutionWorker<WkHtmlToXSession> CreateDefaultWorker(
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
            configuration,
            new LibraryLoaderFactory());

        var options = new ExecutionWorkerOptions(
            name: WorkerName,
            useStaThread: true);

        return new ExecutionWorker<WkHtmlToXSession>(sessionFactory, options);
    }
}
