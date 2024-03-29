using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Exceptions;
using AdaskoTheBeAsT.WkHtmlToX.Settings;
using AdaskoTheBeAsT.WkHtmlToX.Utils;

namespace AdaskoTheBeAsT.WkHtmlToX.Engine;

internal sealed class PdfProcessor
    : ProcessorBase,
        IPdfProcessor
{
    public PdfProcessor(
        WkHtmlToXConfiguration configuration,
        IWkHtmlToPdfModule pdfModule)
        : base(configuration)
    {
        PdfModule = pdfModule ?? throw new ArgumentNullException(nameof(pdfModule));
    }

    public IWkHtmlToPdfModule PdfModule { get; }

    public bool Convert(IHtmlToPdfDocument document, Func<int, Stream> createStreamFunc)
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

#if NETSTANDARD2_0
        if (createStreamFunc is null)
        {
            throw new ArgumentNullException(nameof(createStreamFunc));
        }
#endif
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(createStreamFunc);
#endif

        if (document.ObjectSettings.Count == 0)
        {
            throw new ArgumentException(
                "No objects is defined in document that was passed. At least one object must be defined.");
        }

        ProcessingDocument = document;

        var (converterPtr, globalSettingsPtr, objectSettingsPtrs) = CreateConverter(document);

        RegisterEvents(converterPtr);

        try
        {
            var converted = PdfModule.Convert(converterPtr);

            if (converted)
            {
                PdfModule.GetOutput(converterPtr, createStreamFunc);
            }

            return converted;
        }
        finally
        {
            for (int i = objectSettingsPtrs.Count - 1; i >= 0; i--)
            {
                PdfModule.DestroyObjectSetting(objectSettingsPtrs[i]);
            }

            PdfModule.DestroyGlobalSetting(globalSettingsPtr);
            PdfModule.DestroyConverter(converterPtr);

            ProcessingDocument = null;
        }
    }

    internal (IntPtr converterPtr, IntPtr globalSettingsPtr, List<IntPtr> objectSettingsPtrs) CreateConverter(
        IHtmlToPdfDocument document)
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

        var globalSettings = PdfModule.CreateGlobalSettings();
        ApplyConfig(globalSettings, document.GlobalSettings, useGlobal: true);
        var converter = PdfModule.CreateConverter(globalSettings);
        var objectSettingsPtr = new List<IntPtr>();
        foreach (var obj in document.ObjectSettings)
        {
            if (obj == null)
            {
                continue;
            }

            var objectSettings = PdfModule.CreateObjectSettings();
            objectSettingsPtr.Add(objectSettings);

            ApplyConfig(objectSettings, obj, useGlobal: false);

            AddContent(converter, objectSettings, obj);
        }

        return (converter, globalSettings, objectSettingsPtr);
    }

    internal void AddContent(
        IntPtr converter,
        IntPtr objectSettings,
        PdfObjectSettings pdfObjectSettings)
    {
#if NETSTANDARD2_0
        if (pdfObjectSettings is null)
        {
            throw new ArgumentNullException(nameof(pdfObjectSettings));
        }
#endif
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(pdfObjectSettings);
#endif
        if (!string.IsNullOrEmpty(pdfObjectSettings.HtmlContent))
        {
            AddContentString(converter, objectSettings, pdfObjectSettings);
        }
        else if (pdfObjectSettings.HtmlContentByteArray != null)
        {
            AddContentByteArray(converter, objectSettings, pdfObjectSettings.HtmlContentByteArray);
        }
        else if (pdfObjectSettings.HtmlContentStream != null)
        {
            AddContentStream(converter, objectSettings, pdfObjectSettings.HtmlContentStream);
        }
        else
        {
            throw new HtmlContentEmptyException(
                $"pdfObjectSettings should have non-empty {nameof(PdfObjectSettings.HtmlContent)}"
                + $" or {nameof(PdfObjectSettings.HtmlContentByteArray)} or {nameof(PdfObjectSettings.HtmlContentStream)}");
        }
    }

    internal void AddContentString(
        IntPtr converter,
        IntPtr objectSettings,
        PdfObjectSettings pdfObjectSettings)
    {
#if NETSTANDARD2_0
        if (pdfObjectSettings is null)
        {
            throw new ArgumentNullException(nameof(pdfObjectSettings));
        }
#endif
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(pdfObjectSettings);
#endif

        if (string.IsNullOrEmpty(pdfObjectSettings.HtmlContent))
        {
            throw new ArgumentException("Html content should not be empty");
        }

        var encoding = pdfObjectSettings.Encoding ?? Encoding.UTF8;
        var length = encoding.GetByteCount(pdfObjectSettings.HtmlContent ?? string.Empty);
        var buffer = ArrayPool<byte>.Shared.Rent(length + 1);
        buffer[length] = 0;

        try
        {
            encoding.GetBytes(
                pdfObjectSettings.HtmlContent ?? string.Empty, 0, pdfObjectSettings.HtmlContent!.Length, buffer, 0);
            PdfModule.AddObject(converter, objectSettings, buffer);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    internal void AddContentByteArray(
        IntPtr converter,
        IntPtr objectSettings,
        byte[] htmlContentByteArray)
    {
        PdfModule.AddObject(converter, objectSettings, htmlContentByteArray);
    }

    internal void AddContentStream(
        IntPtr converter,
        IntPtr objectSettings,
        Stream htmlContentStream)
    {
#if NETSTANDARD2_0
        if (htmlContentStream is null)
        {
            throw new ArgumentNullException(nameof(htmlContentStream));
        }
#endif
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(htmlContentStream);
#endif

        var length = htmlContentStream.Length;
        if (length > int.MaxValue)
        {
            throw new HtmlContentStreamTooLargeException();
        }

        var len = (int)length;

        var buffer = ArrayPool<byte>.Shared.Rent(len);
        try
        {
            _ = htmlContentStream.Read(buffer, 0, len);
            PdfModule.AddObject(converter, objectSettings, buffer);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    protected internal override Func<IntPtr, string, string?, int> GetApplySettingFunc(bool useGlobal)
    {
        if (useGlobal)
        {
            return PdfModule.SetGlobalSetting;
        }

        return PdfModule.SetObjectSetting;
    }

    protected internal override int GetCurrentPhase(IntPtr converter) =>
        PdfModule.GetCurrentPhase(converter);

    protected internal override int GetPhaseCount(IntPtr converter) => PdfModule.GetPhaseCount(converter);

    protected internal override string GetPhaseDescription(
        IntPtr converter,
        int phase) =>
        PdfModule.GetPhaseDescription(converter, phase);

    protected internal override string GetProgressDescription(IntPtr converter) =>
        PdfModule.GetProgressDescription(converter);

    protected internal override int SetWarningCallback(
        IntPtr converter,
        StringCallback callback) =>
        PdfModule.SetWarningCallback(converter, callback);

    protected internal override int SetErrorCallback(
        IntPtr converter,
        StringCallback callback) =>
        PdfModule.SetErrorCallback(converter, callback);

    protected internal override int SetPhaseChangedCallback(
        IntPtr converter,
        VoidCallback callback) =>
        PdfModule.SetPhaseChangedCallback(converter, callback);

    protected internal override int SetProgressChangedCallback(
        IntPtr converter,
        VoidCallback callback) =>
        PdfModule.SetProgressChangedCallback(converter, callback);

    protected internal override int SetFinishedCallback(
        IntPtr converter,
        IntCallback callback) =>
        PdfModule.SetFinishedCallback(converter, callback);
}
