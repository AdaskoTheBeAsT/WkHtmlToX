using System;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Interop.Execution;

namespace AdaskoTheBeAsT.WkHtmlToX.Engine;

internal sealed class WkHtmlToXWorker : IExecutionWorker<WkHtmlToXSession>
{
    private readonly ExecutionWorker<WkHtmlToXSession> _worker;
    private readonly WkHtmlToXSessionFactory? _nativeFactory;
    private WkHtmlToXEngine? _engine;

    internal WkHtmlToXWorker(
        IExecutionSessionFactory<WkHtmlToXSession> sessionFactory,
        ExecutionWorkerOptions options)
    {
        ShutdownMode = options.ShutdownMode;
        DisposeTimeout = options.DisposeTimeout;
        _nativeFactory = sessionFactory as WkHtmlToXSessionFactory;
        _nativeFactory?.ReserveOwnership();
        try
        {
            _worker = new ExecutionWorker<WkHtmlToXSession>(sessionFactory, options);
        }
        catch
        {
            _nativeFactory?.ReleaseOwnership();
            throw;
        }
    }

    public event EventHandler<WorkerFaultedEventArgs>? WorkerFaulted
    {
        add => _worker.WorkerFaulted += value;
        remove => _worker.WorkerFaulted -= value;
    }

    public bool IsFaulted => _worker.IsFaulted;

    public Exception? Fault => _worker.Fault;

    public int QueueDepth => _worker.QueueDepth;

    public string? Name => _worker.Name;

    internal ExecutionShutdownMode ShutdownMode { get; }

    internal TimeSpan DisposeTimeout { get; }

    public ExecutionWorkerSnapshot GetSnapshot() => _worker.GetSnapshot();

    public void Initialize()
    {
        ThrowIfReentrant();
        _worker.Initialize();
    }

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfReentrant();
        return _worker.InitializeAsync(cancellationToken);
    }

    public Task ExecuteAsync(
        Action<WkHtmlToXSession, CancellationToken> action,
        ExecutionRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ThrowIfReentrant();
        return _worker.ExecuteAsync(action, options, cancellationToken);
    }

    public Task<TResult> ExecuteAsync<TResult>(
        Func<WkHtmlToXSession, CancellationToken, TResult> action,
        ExecutionRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ThrowIfReentrant();
        return _worker.ExecuteAsync(action, options, cancellationToken);
    }

    public void Dispose()
    {
        ThrowIfReentrant();
        if (_engine is not null)
        {
#pragma warning disable IDISP007 // Route container disposal through the pipeline coordinator, not around it.
            _engine.Dispose();
#pragma warning restore IDISP007
            return;
        }

        _worker.Dispose();

        // A synchronous timeout abandons only this wait. Keep the lease until exit.
        _ = DisposeAsync().AsTask();
    }

#pragma warning disable IDISP007 // The engine coordinates disposal of this worker and its admitted pipeline.
    public ValueTask DisposeAsync() => _engine?.DisposeAsync() ?? DisposeNativeAsync();
#pragma warning restore IDISP007

    internal void CoordinateShutdown(WkHtmlToXEngine engine) => _engine = engine;

    internal async ValueTask DisposeNativeAsync()
    {
        ThrowIfReentrant();
        await _worker.DisposeAsync().ConfigureAwait(false);
        _nativeFactory?.ReleaseOwnership();
    }

    internal void ThrowIfReentrant()
    {
        if (_nativeFactory?.IsCurrentThread == true)
        {
            throw new InvalidOperationException("Native callbacks and stream operations must not reenter the engine or its lifecycle.");
        }
    }
}
