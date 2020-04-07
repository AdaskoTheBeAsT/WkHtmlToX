using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Exceptions;
using AdaskoTheBeAsT.WkHtmlToX.Modules;
using AdaskoTheBeAsT.WkHtmlToX.Settings;

namespace AdaskoTheBeAsT.WkHtmlToX
{
    public abstract class PdfConverterBase
        : ConverterBase
    {
#pragma warning disable SA1401 // Fields should be private
#pragma warning disable CA1051 // Do not declare visible instance fields
        internal readonly IWkHtmlToPdfModule _pdfModule;
#pragma warning restore CA1051 // Do not declare visible instance fields
#pragma warning restore SA1401 // Fields should be private

#pragma warning disable S3442 // "abstract" classes should not have "public" constructors
        internal PdfConverterBase(
            IWkHtmlToXModuleFactory moduleFactory,
            IWkHtmlToPdfModuleFactory pdfModuleFactory)
            : base(moduleFactory, ModuleKind.Pdf)
        {
            if (pdfModuleFactory is null)
            {
                throw new ArgumentNullException(nameof(pdfModuleFactory));
            }

            _pdfModule = pdfModuleFactory.GetModule((int)Environment.OSVersion.Platform);
        }
#pragma warning restore S3442 // "abstract" classes should not have "public" constructors

        [ExcludeFromCodeCoverage]
        protected PdfConverterBase()
            : this(
                new WkHtmlToXModuleFactory(),
                new WkHtmlToPdfModuleFactory())
        {
        }

        protected internal (IntPtr converterPtr, IntPtr globalSettingsPtr, List<IntPtr> objectSettingsPtrs) CreateConverter(
            IHtmlToPdfDocument document)
        {
            if (document is null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var globalSettings = _module.CreateGlobalSettings();
            ApplyConfig(globalSettings, document.GlobalSettings, true);
            var converter = _module.CreateConverter(globalSettings);
            var objectSettingsPtr = new List<IntPtr>();
            foreach (var obj in document.ObjectSettings)
            {
                if (obj == null)
                {
                    continue;
                }

                var objectSettings = _pdfModule.CreateObjectSettings();
                objectSettingsPtr.Add(objectSettings);

                ApplyConfig(objectSettings, obj, false);

                AddContent(converter, objectSettings, obj);
            }

            return (converter, globalSettings, objectSettingsPtr);
        }

        protected internal override Func<IntPtr, string, string?, int> GetApplySettingFunc(bool isGlobal)
        {
            if (isGlobal)
            {
                return _module.SetGlobalSetting;
            }

            return _pdfModule.SetObjectSetting;
        }

        protected internal bool ConvertImpl(IHtmlToPdfDocument document, Func<int, Stream> createStreamFunc)
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

            var loaded = _module.Initialize(0) == 1;
            if (!loaded)
            {
                throw new ArgumentException("Not loaded");
            }

            var (converterPtr, globalSettingsPtr, objectSettingsPtrs) = CreateConverter(document);

            RegisterEvents(converterPtr);

            try
            {
                var converted = _module.Convert(converterPtr);

                if (converted)
                {
                    _module.GetOutput(converterPtr, createStreamFunc);
                }

                return converted;
            }
            finally
            {
                for (int i = objectSettingsPtrs.Count - 1; i >= 0; i--)
                {
                    _pdfModule.DestroyObjectSetting(objectSettingsPtrs[i]);
                }

                _module.DestroyGlobalSetting(globalSettingsPtr);
                _module.DestroyConverter(converterPtr);
                _module.Terminate();
                ProcessingDocument = null;
            }
        }

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
            var length = encoding.GetByteCount(pdfObjectSettings.HtmlContent);
            var buffer = ArrayPool<byte>.Shared.Rent(length + 1);
            buffer[length] = 0;

            try
            {
                encoding.GetBytes(pdfObjectSettings.HtmlContent, 0, pdfObjectSettings.HtmlContent!.Length, buffer, 0);
                _pdfModule.AddObject(converter, objectSettings, buffer);
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
            _pdfModule.AddObject(converter, objectSettings, htmlContentByteArray);
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
                htmlContentStream.Read(buffer, 0, len);
                _pdfModule.AddObject(converter, objectSettings, buffer);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}
