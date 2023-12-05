using System;
using AdaskoTheBeAsT.WkHtmlToX.EventDefinitions;
using AdaskoTheBeAsT.WkHtmlToX.Loaders;

namespace AdaskoTheBeAsT.WkHtmlToX.Engine;

public sealed class WkHtmlToXConfiguration
{
    public WkHtmlToXConfiguration(
        int platformId,
        WkHtmlToXRuntimeIdentifier? runtimeIdentifier)
    {
        PlatformId = platformId;
        RuntimeIdentifier = runtimeIdentifier;
    }

    public int PlatformId { get; }

    public WkHtmlToXRuntimeIdentifier? RuntimeIdentifier { get; }

    public Action<ErrorEventArgs>? ErrorAction { get; set; }

    public Action<FinishedEventArgs>? FinishedAction { get; set; }

    public Action<PhaseChangedEventArgs>? PhaseChangedAction { get; set; }

    public Action<ProgressChangedEventArgs>? ProgressChangedAction { get; set; }

    public Action<WarningEventArgs>? WarningAction { get; set; }
}
