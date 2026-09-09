using System;
using AdaskoTheBeAsT.WkHtmlToX.EventDefinitions;
using AdaskoTheBeAsT.WkHtmlToX.Loaders;

namespace AdaskoTheBeAsT.WkHtmlToX.Engine;

public sealed class WkHtmlToXConfiguration
{
    public WkHtmlToXConfiguration()
        : this(
            GetPlatformId(),
            runtimeIdentifier: null)
    {
    }

    public WkHtmlToXConfiguration(
        int platformId,
        WkHtmlToXRuntimeIdentifier? runtimeIdentifier)
    {
        PlatformId = platformId;
        RuntimeIdentifier = runtimeIdentifier;
    }

    public int PlatformId { get; }

    public WkHtmlToXRuntimeIdentifier? RuntimeIdentifier { get; }

    /// <summary>Gets or sets an absolute path to a trusted wkhtmltox native library.</summary>
    public string? NativeLibraryPath { get; set; }

    public WkHtmlToXRequestOptions RequestOptions { get; set; } = new();

    /// <summary>Gets or sets worker policy for both standalone and DI/hosted engines.</summary>
    public WkHtmlToXWorkerOptions WorkerOptions { get; set; } = new();

    public Action<ErrorEventArgs>? ErrorAction { get; set; }

    public Action<FinishedEventArgs>? FinishedAction { get; set; }

    public Action<PhaseChangedEventArgs>? PhaseChangedAction { get; set; }

    public Action<ProgressChangedEventArgs>? ProgressChangedAction { get; set; }

    public Action<WarningEventArgs>? WarningAction { get; set; }

    internal WkHtmlToXConfiguration Snapshot()
    {
        if (RequestOptions is null || WorkerOptions is null)
        {
            throw new ArgumentException("RequestOptions and WorkerOptions must not be null.");
        }

        return new(PlatformId, RuntimeIdentifier)
        {
            ErrorAction = ErrorAction,
            FinishedAction = FinishedAction,
            PhaseChangedAction = PhaseChangedAction,
            ProgressChangedAction = ProgressChangedAction,
            WarningAction = WarningAction,
            NativeLibraryPath = NativeLibraryPath,
            RequestOptions = RequestOptions.Snapshot(),
            WorkerOptions = WorkerOptions.Snapshot(),
        };
    }

    private static int GetPlatformId()
    {
#if NET8_0_OR_GREATER
        if (OperatingSystem.IsWindows())
#else
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
#endif
        {
            return (int)PlatformID.Win32NT;
        }

#if NET8_0_OR_GREATER
        return OperatingSystem.IsMacOS() ? (int)PlatformID.MacOSX : (int)PlatformID.Unix;
#else
        return System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX)
            ? (int)PlatformID.MacOSX
            : (int)PlatformID.Unix;
#endif
    }
}
