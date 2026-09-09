using System;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Interop.Execution;

namespace AdaskoTheBeAsT.WkHtmlToX.Engine;

public sealed partial class WkHtmlToXEngine
{
    private Task? _shutdownTask;
    private bool _cancelPending;

    /// <summary>Stops admission and joins the whole pipeline using the configured worker shutdown policy.</summary>
    /// <remarks>Cancellation limits only this wait. Repeated calls join the same shutdown.</remarks>
    /// <param name="cancellationToken">Cancels only the caller's wait.</param>
    public Task ShutdownAsync(CancellationToken cancellationToken = default) =>
        ShutdownAsync((_worker as WkHtmlToXWorker)?.ShutdownMode ?? ExecutionShutdownMode.Drain, cancellationToken);

    /// <summary>Stops admission and joins input, native execution, delivery, and owned worker teardown.</summary>
    /// <remarks>The first shutdown call selects the policy. CancelPending never interrupts active native work or stream I/O.</remarks>
    /// <param name="shutdownMode">Whether to drain admitted requests or skip requests not yet in native execution.</param>
    /// <param name="cancellationToken">Cancels only the caller's wait.</param>
    public Task ShutdownAsync(ExecutionShutdownMode shutdownMode, CancellationToken cancellationToken = default)
    {
        (_worker as WkHtmlToXWorker)?.ThrowIfReentrant();
        if (shutdownMode != ExecutionShutdownMode.Drain && shutdownMode != ExecutionShutdownMode.CancelPending)
        {
            throw new ArgumentOutOfRangeException(nameof(shutdownMode));
        }

        Task shutdown;
        lock (_admissionLock)
        {
            if (_shutdownTask is null)
            {
                _stopped = true;
                _cancelPending = shutdownMode == ExecutionShutdownMode.CancelPending;
                if (_activeRequests == 0)
                {
                    _requestsExited.TrySetResult(true);
                }

                _shutdownTask = Task.Run(ShutdownCoreAsync, CancellationToken.None);
            }

            shutdown = _shutdownTask;
        }

        return cancellationToken.CanBeCanceled ? WaitForShutdownAsync(shutdown, cancellationToken) : shutdown;
    }

    public void Dispose()
    {
        var shutdown = ShutdownAsync();
        var timeout = (_worker as WkHtmlToXWorker)?.DisposeTimeout ?? Timeout.InfiniteTimeSpan;
#pragma warning disable VSTHRD002 // Synchronous compatibility API bounds only its wait, not pipeline ownership.
        if (timeout == Timeout.InfiniteTimeSpan
            || Task.WhenAny(shutdown, Task.Delay(timeout)).GetAwaiter().GetResult() == shutdown)
        {
            shutdown.GetAwaiter().GetResult();
        }
#pragma warning restore VSTHRD002
    }

    public ValueTask DisposeAsync() => new(ShutdownAsync());

    private static async Task WaitForShutdownAsync(Task shutdown, CancellationToken cancellationToken)
    {
        var canceled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
#if NET8_0_OR_GREATER
        await using var registration = cancellationToken.Register(() => canceled.TrySetCanceled(cancellationToken)).ConfigureAwait(false);
#else
        using var registration = cancellationToken.Register(() => canceled.TrySetCanceled(cancellationToken));
#endif
#pragma warning disable VSTHRD003 // Cancellation abandons only this caller's wait on shared shutdown.
        var completed = await Task.WhenAny(shutdown, canceled.Task).ConfigureAwait(false);
        await completed.ConfigureAwait(false);
#pragma warning restore VSTHRD003
    }

    private async Task ShutdownCoreAsync()
    {
#pragma warning disable VSTHRD003 // Join all pipeline stages before closing worker admission or releasing native ownership.
        await _requestsExited.Task.ConfigureAwait(false);
#pragma warning restore VSTHRD003
        if (_ownsWorker && _worker is WkHtmlToXWorker nativeWorker)
        {
            await nativeWorker.DisposeNativeAsync().ConfigureAwait(false);
        }
        else if (_ownsWorker)
        {
#pragma warning disable IDISP007 // Ownership is explicit; DI registers the engine as the worker lifecycle coordinator.
            await _worker.DisposeAsync().ConfigureAwait(false);
#pragma warning restore IDISP007
        }
    }

    private bool IsPendingCanceled()
    {
        lock (_admissionLock)
        {
            return _cancelPending;
        }
    }

    private void ThrowIfPendingCanceled()
    {
        if (IsPendingCanceled())
        {
            throw new OperationCanceledException(new CancellationToken(canceled: true));
        }
    }
}
