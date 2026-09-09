using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Utils;

namespace AdaskoTheBeAsT.WkHtmlToX.Engine;

internal sealed class ImageProcessor
    : ProcessorBase,
        IImageProcessor
{
    public ImageProcessor(WkHtmlToXConfiguration configuration, IWkHtmlToImageModule imageModule)
        : base(configuration)
    {
        ImageModule = imageModule ?? throw new ArgumentNullException(nameof(imageModule));
    }

    public IWkHtmlToImageModule ImageModule { get; }

    public bool Convert(IHtmlToImageDocument? document, Func<int, Stream> createStreamFunc)
        => ConvertWithResult(document, createStreamFunc, CancellationToken.None).ToLegacyResult();

    public ConversionResult ConvertWithResult(
        IHtmlToImageDocument? document,
        Func<int, Stream> createStreamFunc,
        CancellationToken cancellationToken)
    {
#if !NET8_0_OR_GREATER
#pragma warning disable RCS1256 // Invalid argument null check
        if (document is null)
        {
            throw new ArgumentNullException(nameof(document));
        }
#pragma warning restore RCS1256 // Invalid argument null check
#endif
#if NET8_0_OR_GREATER
#pragma warning disable RCS1256 // Invalid argument null check
        ArgumentNullException.ThrowIfNull(document);
#pragma warning restore RCS1256 // Invalid argument null check
#endif

        if (document.ImageSettings is null)
        {
            throw new ArgumentException(
                "No image settings is defined in document that was passed. At least one object must be defined.");
        }

#if !NET8_0_OR_GREATER
        if (createStreamFunc is null)
        {
            throw new ArgumentNullException(nameof(createStreamFunc));
        }
#endif
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(createStreamFunc);
#endif

        return ExecuteConversion(
            document,
            () => CreateConverter(document).converterPtr,
            ImageModule,
            createStreamFunc,
            cancellationToken);
    }

    internal (IntPtr converterPtr, IntPtr globalSettingsPtr) CreateConverter(
        IHtmlToImageDocument document)
    {
#if !NET8_0_OR_GREATER
        if (document is null)
        {
            throw new ArgumentNullException(nameof(document));
        }
#endif
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(document);
#endif
        var globalSettings = IntPtr.Zero;
        var converter = IntPtr.Zero;
        try
        {
            globalSettings = ImageModule.CreateGlobalSettings();
            ApplyConfig(globalSettings, document.ImageSettings, useGlobal: true);
            converter = ImageModule.CreateConverter(globalSettings);
            EnsureConverterCreated(converter);

            return (converter, globalSettings);
        }
        catch (Exception exception)
        {
            CleanupFailedCreateConverter(converter, globalSettings, exception);
            throw;
        }
    }

    protected internal override Func<IntPtr, string, string?, int> GetApplySettingFunc(bool useGlobal) =>
        ImageModule.SetGlobalSetting;

    protected internal override int GetCurrentPhase(IntPtr converter) => ImageModule.GetCurrentPhase(converter);

    protected internal override int GetPhaseCount(IntPtr converter) => ImageModule.GetPhaseCount(converter);

    protected internal override string GetPhaseDescription(
        IntPtr converter,
        int phase) =>
        ImageModule.GetPhaseDescription(converter, phase);

    protected internal override string GetProgressDescription(IntPtr converter) =>
        ImageModule.GetProgressDescription(converter);

    protected internal override void SetWarningCallback(
        IntPtr converter,
        StringCallback callback) =>
        ImageModule.SetWarningCallback(converter, callback);

    protected internal override void SetErrorCallback(
        IntPtr converter,
        StringCallback callback) =>
        ImageModule.SetErrorCallback(converter, callback);

    protected internal override void SetPhaseChangedCallback(
        IntPtr converter,
        VoidCallback callback) =>
        ImageModule.SetPhaseChangedCallback(converter, callback);

    protected internal override void SetProgressChangedCallback(
        IntPtr converter,
        IntCallback callback) =>
        ImageModule.SetProgressChangedCallback(converter, callback);

    protected internal override void SetFinishedCallback(
        IntPtr converter,
        IntCallback callback) =>
        ImageModule.SetFinishedCallback(converter, callback);

    private static void EnsureConverterCreated(IntPtr converter)
    {
        if (converter == IntPtr.Zero)
        {
            throw new ArgumentException("converter pointer cannot be zero", nameof(converter));
        }
    }

    private void CleanupFailedCreateConverter(IntPtr converter, IntPtr globalSettings, Exception originalFailure)
    {
        var failures = new List<Exception>();
        if (converter != IntPtr.Zero)
        {
            NativeCleanup.Attempt(() => ImageModule.DestroyConverter(converter), failures);
        }
        else if (globalSettings != IntPtr.Zero)
        {
            NativeCleanup.Attempt(() => ImageModule.DestroyGlobalSetting(globalSettings), failures);
        }

        NativeCleanup.ThrowIfFailed(failures, originalFailure);
    }
}
