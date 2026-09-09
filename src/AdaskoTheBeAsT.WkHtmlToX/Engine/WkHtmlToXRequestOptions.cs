using System;

namespace AdaskoTheBeAsT.WkHtmlToX.Engine;

/// <summary>Managed request limits, snapshotted when the engine is created.</summary>
public sealed class WkHtmlToXRequestOptions
{
    /// <summary>Gets or sets the maximum requests in input, native execution, or output delivery combined.</summary>
    public int MaxConcurrentRequests { get; set; } = 32;

    /// <summary>Gets or sets the maximum inline HTML bytes per PDF request, excluding library-added terminators.</summary>
    public int MaxInputBytes { get; set; } = 32 * 1024 * 1024;

    /// <summary>Gets or sets the aggregate inline input bytes reserved across admitted PDF requests.</summary>
    public long MaxBufferedInputBytes { get; set; } = 128 * 1024 * 1024;

    /// <summary>Gets or sets the maximum managed output bytes per request.</summary>
    public int MaxOutputBytes { get; set; } = 64 * 1024 * 1024;

    /// <summary>Gets or sets the maximum detached output bytes across all requests.</summary>
    public long MaxBufferedOutputBytes { get; set; } = 128 * 1024 * 1024;

    internal WkHtmlToXRequestOptions Snapshot()
    {
        if (MaxConcurrentRequests <= 0 || MaxInputBytes <= 0 || MaxInputBytes == int.MaxValue
            || MaxBufferedInputBytes < MaxInputBytes
            || MaxOutputBytes <= 0 || MaxBufferedOutputBytes < MaxOutputBytes)
        {
            throw new ArgumentException("Request limits must be positive and shared budgets must cover one maximum-sized request.");
        }

        return (WkHtmlToXRequestOptions)MemberwiseClone();
    }
}
