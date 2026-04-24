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
#if !NET8_0_OR_GREATER
        if (document is null)
        {
            throw new ArgumentNullException(nameof(document));
        }
#endif
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(document);
#endif

#if !NET8_0_OR_GREATER
        if (createStreamFunc is null)
        {
            throw new ArgumentNullException(nameof(createStreamFunc));
        }
#endif
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(createStreamFunc);
#endif

        if (document.ObjectSettings.Count == 0)
        {
            throw new ArgumentException(
                "No objects is defined in document that was passed. At least one object must be defined.");
        }

        ProcessingDocument = document;

        var converterPtr = IntPtr.Zero;
        try
        {
            var converterData = CreateConverter(document);
            converterPtr = converterData.converterPtr;

            RegisterEvents(converterPtr);

            var converted = PdfModule.Convert(converterPtr);

            if (converted)
            {
                PdfModule.GetOutput(converterPtr, createStreamFunc);
            }

            return converted;
        }
        finally
        {
            if (converterPtr != IntPtr.Zero)
            {
                PdfModule.DestroyConverter(converterPtr);
            }

            ReleaseRegisteredCallbacks();
            ProcessingDocument = null;
        }
    }

    internal (IntPtr converterPtr, IntPtr globalSettingsPtr, List<IntPtr> objectSettingsPtrs) CreateConverter(
        IHtmlToPdfDocument document)
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
        var objectSettingsPtr = new List<IntPtr>();
        var unattachedObjectSettingsPtr = new List<IntPtr>();
        try
        {
            globalSettings = PdfModule.CreateGlobalSettings();
            ApplyConfig(globalSettings, document.GlobalSettings, useGlobal: true);
            converter = PdfModule.CreateConverter(globalSettings);
            EnsureConverterCreated(converter);
            foreach (var obj in document.ObjectSettings)
            {
                if (obj == null)
                {
                    continue;
                }

                var objectSettings = PdfModule.CreateObjectSettings();
                objectSettingsPtr.Add(objectSettings);
                unattachedObjectSettingsPtr.Add(objectSettings);

                ApplyConfig(objectSettings, obj, useGlobal: false);

                AddContent(converter, objectSettings, obj);
                unattachedObjectSettingsPtr.Remove(objectSettings);
            }

            return (converter, globalSettings, objectSettingsPtr);
        }
        catch
        {
            CleanupFailedCreateConverter(converter, globalSettings, unattachedObjectSettingsPtr);
            throw;
        }
    }

    internal void AddContent(
        IntPtr converter,
        IntPtr objectSettings,
        PdfObjectSettings pdfObjectSettings)
    {
#if !NET8_0_OR_GREATER
        if (pdfObjectSettings is null)
        {
            throw new ArgumentNullException(nameof(pdfObjectSettings));
        }
#endif
#if NET8_0_OR_GREATER
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
#if !NET8_0_OR_GREATER
        if (pdfObjectSettings is null)
        {
            throw new ArgumentNullException(nameof(pdfObjectSettings));
        }
#endif
#if NET8_0_OR_GREATER
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
        if (htmlContentByteArray.Length > 0 && htmlContentByteArray[htmlContentByteArray.Length - 1] == byte.MinValue)
        {
            PdfModule.AddObject(converter, objectSettings, htmlContentByteArray);
            return;
        }

        var terminated = new byte[htmlContentByteArray.Length + 1];
        Array.Copy(htmlContentByteArray, terminated, htmlContentByteArray.Length);
        terminated[terminated.Length - 1] = byte.MinValue;
        PdfModule.AddObject(converter, objectSettings, terminated);
    }

    internal void AddContentStream(
        IntPtr converter,
        IntPtr objectSettings,
        Stream htmlContentStream)
    {
#if !NET8_0_OR_GREATER
        if (htmlContentStream is null)
        {
            throw new ArgumentNullException(nameof(htmlContentStream));
        }
#endif
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(htmlContentStream);
#endif

        var length = htmlContentStream.Length - htmlContentStream.Position;
        if (length < 0)
        {
            throw new ArgumentException(
                "The stream position is greater than the stream length.",
                nameof(htmlContentStream));
        }

        if (length > int.MaxValue)
        {
            throw new HtmlContentStreamTooLargeException();
        }

        var len = (int)length;

        var buffer = ArrayPool<byte>.Shared.Rent(len + 1);
        try
        {
            ReadExact(htmlContentStream, buffer, len);
            buffer[len] = 0;
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

    protected internal override void SetWarningCallback(
        IntPtr converter,
        StringCallback callback) =>
        PdfModule.SetWarningCallback(converter, callback);

    protected internal override void SetErrorCallback(
        IntPtr converter,
        StringCallback callback) =>
        PdfModule.SetErrorCallback(converter, callback);

    protected internal override void SetPhaseChangedCallback(
        IntPtr converter,
        VoidCallback callback) =>
        PdfModule.SetPhaseChangedCallback(converter, callback);

    protected internal override void SetProgressChangedCallback(
        IntPtr converter,
        IntCallback callback) =>
        PdfModule.SetProgressChangedCallback(converter, callback);

    protected internal override void SetFinishedCallback(
        IntPtr converter,
        IntCallback callback) =>
        PdfModule.SetFinishedCallback(converter, callback);

    private static void ReadExact(Stream htmlContentStream, byte[] buffer, int length)
    {
        var bytesRead = 0;
        while (bytesRead < length)
        {
            var read = htmlContentStream.Read(buffer, bytesRead, length - bytesRead);
            if (read == 0)
            {
                throw new EndOfStreamException(
                    "Could not read all bytes from htmlContentStream.");
            }

            bytesRead += read;
        }
    }

    private static void EnsureConverterCreated(IntPtr converter)
    {
        if (converter == IntPtr.Zero)
        {
            throw new ArgumentException("converter pointer cannot be zero", nameof(converter));
        }
    }

    private void CleanupFailedCreateConverter(
        IntPtr converter,
        IntPtr globalSettings,
        List<IntPtr> unattachedObjectSettingsPtr)
    {
        foreach (var objectSettings in unattachedObjectSettingsPtr)
        {
            if (objectSettings != IntPtr.Zero)
            {
                PdfModule.DestroyObjectSetting(objectSettings);
            }
        }

        if (converter != IntPtr.Zero)
        {
            PdfModule.DestroyConverter(converter);
        }
        else if (globalSettings != IntPtr.Zero)
        {
            PdfModule.DestroyGlobalSetting(globalSettings);
        }
    }
}
