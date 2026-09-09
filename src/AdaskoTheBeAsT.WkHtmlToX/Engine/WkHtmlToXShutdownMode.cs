namespace AdaskoTheBeAsT.WkHtmlToX.Engine;

/// <summary>Shutdown policy for the whole conversion pipeline, not a native execution deadline.</summary>
public enum WkHtmlToXShutdownMode
{
    /// <summary>Finish all admitted requests before tearing down the native worker.</summary>
    Drain,

    /// <summary>Skip requests not yet in native execution without interrupting active native work or stream I/O.</summary>
    CancelPending,
}
