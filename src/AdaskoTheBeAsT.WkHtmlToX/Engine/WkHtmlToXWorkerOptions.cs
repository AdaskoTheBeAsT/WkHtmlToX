using System;
using System.Threading;
using AdaskoTheBeAsT.Interop.Execution;

namespace AdaskoTheBeAsT.WkHtmlToX.Engine;

/// <summary>Native worker policy, snapshotted at engine construction or DI registration.</summary>
public sealed class WkHtmlToXWorkerOptions
{
    /// <summary>Gets or sets the worker name used by diagnostics. Do not include document data or credentials.</summary>
    public string? Name { get; set; } = "WkHtmlToX Engine Worker";

    /// <summary>Gets or sets the periodic session recycling interval. Zero disables periodic recycling.</summary>
    public int MaxOperationsPerSession { get; set; }

    /// <summary>
    /// Gets or sets the synchronous disposal wait limit for the whole pipeline.
    /// Defaults to an infinite wait. A timeout does not interrupt work or release its resources.
    /// </summary>
    public TimeSpan DisposeTimeout { get; set; } = Timeout.InfiniteTimeSpan;

    /// <summary>Gets or sets the default policy used by shutdown, disposal, and host stop.</summary>
    public WkHtmlToXShutdownMode ShutdownMode { get; set; }

    internal WkHtmlToXWorkerOptions Snapshot()
    {
        var snapshot = (WkHtmlToXWorkerOptions)MemberwiseClone();
        if (snapshot.MaxOperationsPerSession < 0
            || (snapshot.DisposeTimeout < TimeSpan.Zero && snapshot.DisposeTimeout != Timeout.InfiniteTimeSpan)
            || snapshot.DisposeTimeout.TotalMilliseconds > int.MaxValue
            || (snapshot.ShutdownMode != WkHtmlToXShutdownMode.Drain && snapshot.ShutdownMode != WkHtmlToXShutdownMode.CancelPending))
        {
            throw new ArgumentException("Worker options require a nonnegative recycling interval, a valid disposal timeout, and a supported shutdown mode.");
        }

        return snapshot;
    }

    internal void ApplyTo(ExecutionWorkerOptions options)
    {
        options.Name = Name;
        options.UseStaThread = true;
        options.MaxOperationsPerSession = MaxOperationsPerSession;
        options.DisposeTimeout = DisposeTimeout;
        options.ShutdownMode = ShutdownMode == WkHtmlToXShutdownMode.CancelPending
            ? ExecutionShutdownMode.CancelPending
            : ExecutionShutdownMode.Drain;
    }
}
