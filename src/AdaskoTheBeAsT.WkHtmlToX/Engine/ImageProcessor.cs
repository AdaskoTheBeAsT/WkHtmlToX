using System;
using System.IO;
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
    {
#if NETSTANDARD2_0
        if (document?.ImageSettings == null)
        {
            throw new ArgumentException(
                "No image settings is defined in document that was passed. At least one object must be defined.");
        }
#endif
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(document?.ImageSettings);
#endif

#if NETSTANDARD2_0
        if (createStreamFunc is null)
        {
            throw new ArgumentNullException(nameof(createStreamFunc));
        }
#endif
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(createStreamFunc);
#endif

        ProcessingDocument = document;

#pragma warning disable S1481 // Unused local variables should be removed
        // ReSharper disable once UnusedVariable
        var (converterPtr, globalSettingsPtr) = CreateConverter(document);
#pragma warning restore S1481 // Unused local variables should be removed

        RegisterEvents(converterPtr);

        try
        {
            var converted = ImageModule.Convert(converterPtr);

            if (converted)
            {
                ImageModule.GetOutput(converterPtr, createStreamFunc);
            }

            return converted;
        }
        finally
        {
            ImageModule.DestroyConverter(converterPtr);

            // it seems destroying converter also destroys global settings
            ////ImageModule.DestroyGlobalSetting(globalSettingsPtr);
            ProcessingDocument = null;
        }
    }

    internal (IntPtr converterPtr, IntPtr globalSettingsPtr) CreateConverter(
        IHtmlToImageDocument document)
    {
#if NETSTANDARD2_0
        if (document is null)
        {
            throw new ArgumentNullException(nameof(document));
        }
#endif
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(document);
#endif
        var globalSettings = ImageModule.CreateGlobalSettings();
        ApplyConfig(globalSettings, document.ImageSettings, useGlobal: true);
        var converter = ImageModule.CreateConverter(globalSettings);

        return (converter, globalSettings);
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

    protected internal override int SetWarningCallback(
        IntPtr converter,
        StringCallback callback) =>
        ImageModule.SetWarningCallback(converter, callback);

    protected internal override int SetErrorCallback(
        IntPtr converter,
        StringCallback callback) =>
        ImageModule.SetErrorCallback(converter, callback);

    protected internal override int SetPhaseChangedCallback(
        IntPtr converter,
        VoidCallback callback) =>
        ImageModule.SetPhaseChangedCallback(converter, callback);

    protected internal override int SetProgressChangedCallback(
        IntPtr converter,
        VoidCallback callback) =>
        ImageModule.SetProgressChangedCallback(converter, callback);

    protected internal override int SetFinishedCallback(
        IntPtr converter,
        IntCallback callback) =>
        ImageModule.SetFinishedCallback(converter, callback);
}
