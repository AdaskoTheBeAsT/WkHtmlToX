using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Exceptions;
using AdaskoTheBeAsT.WkHtmlToX.Settings;
using AdaskoTheBeAsT.WkHtmlToX.Utils;

namespace AdaskoTheBeAsT.WkHtmlToX.Engine
{
    internal class PdfProcessor
        : ProcessorBase,
            IPdfProcessor
    {
        private readonly IWkHtmlToPdfModule _wkHtmlToPdfModule;

        public PdfProcessor(
            WkHtmlToXConfiguration configuration,
            IWkHtmlToPdfModule wkHtmlToPdfModule)
            : base(configuration)
        {
            _wkHtmlToPdfModule = wkHtmlToPdfModule ?? throw new ArgumentNullException(nameof(wkHtmlToPdfModule));
        }

        public IWkHtmlToPdfModule WkHtmlToPdfModule => _wkHtmlToPdfModule;

        public bool Convert(IHtmlToPdfDocument document, Func<int, Stream> createStreamFunc)
        {
            if (document is null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (createStreamFunc is null)
            {
                throw new ArgumentNullException(nameof(createStreamFunc));
            }

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
                var converted = _wkHtmlToPdfModule.Convert(converterPtr);

                if (converted)
                {
                    _wkHtmlToPdfModule.GetOutput(converterPtr, createStreamFunc);
                }

                return converted;
            }
            finally
            {
                for (int i = objectSettingsPtrs.Count - 1; i >= 0; i--)
                {
                    _wkHtmlToPdfModule.DestroyObjectSetting(objectSettingsPtrs[i]);
                }

                _wkHtmlToPdfModule.DestroyGlobalSetting(globalSettingsPtr);
                _wkHtmlToPdfModule.DestroyConverter(converterPtr);

                ProcessingDocument = null;
            }
        }

        protected internal (IntPtr converterPtr, IntPtr globalSettingsPtr, List<IntPtr> objectSettingsPtrs) CreateConverter(
            IHtmlToPdfDocument document)
        {
            if (document is null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var globalSettings = _wkHtmlToPdfModule.CreateGlobalSettings();
            ApplyConfig(globalSettings, document.GlobalSettings, isGlobal: true);
            var converter = _wkHtmlToPdfModule.CreateConverter(globalSettings);
            var objectSettingsPtr = new List<IntPtr>();
            foreach (var obj in document.ObjectSettings)
            {
                if (obj == null)
                {
                    continue;
                }

                var objectSettings = _wkHtmlToPdfModule.CreateObjectSettings();
                objectSettingsPtr.Add(objectSettings);

                ApplyConfig(objectSettings, obj, isGlobal: false);

                AddContent(converter, objectSettings, obj);
            }

            return (converter, globalSettings, objectSettingsPtr);
        }

        protected internal override Func<IntPtr, string, string?, int> GetApplySettingFunc(bool isGlobal)
        {
            if (isGlobal)
            {
                return _wkHtmlToPdfModule.SetGlobalSetting;
            }

            return _wkHtmlToPdfModule.SetObjectSetting;
        }

        protected internal override int GetCurrentPhase(IntPtr converter) =>
            _wkHtmlToPdfModule.GetCurrentPhase(converter);

        protected internal override int GetPhaseCount(IntPtr converter) => _wkHtmlToPdfModule.GetPhaseCount(converter);

        protected internal override string GetPhaseDescription(
            IntPtr converter,
            int phase) =>
            _wkHtmlToPdfModule.GetPhaseDescription(converter, phase);

        protected internal override string GetProgressDescription(IntPtr converter) =>
            _wkHtmlToPdfModule.GetProgressDescription(converter);

        protected internal override int SetWarningCallback(
            IntPtr converter,
            StringCallback callback) =>
            _wkHtmlToPdfModule.SetWarningCallback(converter, callback);

        protected internal override int SetErrorCallback(
            IntPtr converter,
            StringCallback callback) =>
            _wkHtmlToPdfModule.SetErrorCallback(converter, callback);

        protected internal override int SetPhaseChangedCallback(
            IntPtr converter,
            VoidCallback callback) =>
            _wkHtmlToPdfModule.SetPhaseChangedCallback(converter, callback);

        protected internal override int SetProgressChangedCallback(
            IntPtr converter,
            VoidCallback callback) =>
            _wkHtmlToPdfModule.SetProgressChangedCallback(converter, callback);

        protected internal override int SetFinishedCallback(
            IntPtr converter,
            IntCallback callback) =>
            _wkHtmlToPdfModule.SetFinishedCallback(converter, callback);

        protected internal void AddContent(
            IntPtr converter,
            IntPtr objectSettings,
            PdfObjectSettings pdfObjectSettings)
        {
            if (pdfObjectSettings is null)
            {
                throw new ArgumentNullException(nameof(pdfObjectSettings));
            }

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

        protected internal void AddContentString(
            IntPtr converter,
            IntPtr objectSettings,
            PdfObjectSettings pdfObjectSettings)
        {
            if (pdfObjectSettings is null)
            {
                throw new ArgumentNullException(nameof(pdfObjectSettings));
            }

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
                _wkHtmlToPdfModule.AddObject(converter, objectSettings, buffer);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        protected internal void AddContentByteArray(
            IntPtr converter,
            IntPtr objectSettings,
            byte[] htmlContentByteArray)
        {
            _wkHtmlToPdfModule.AddObject(converter, objectSettings, htmlContentByteArray);
        }

        protected internal void AddContentStream(
            IntPtr converter,
            IntPtr objectSettings,
            Stream htmlContentStream)
        {
            if (htmlContentStream is null)
            {
                throw new ArgumentNullException(nameof(htmlContentStream));
            }

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
                _wkHtmlToPdfModule.AddObject(converter, objectSettings, buffer);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}
