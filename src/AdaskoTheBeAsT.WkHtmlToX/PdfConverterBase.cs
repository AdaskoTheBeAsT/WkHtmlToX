using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Modules;

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

                _pdfModule.AddObject(converter, objectSettings, obj.GetContent());
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

        protected internal Stream ConvertImpl(IHtmlToPdfDocument document)
        {
            if (document is null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (document.ObjectSettings?.Count == 0)
            {
                throw new ArgumentException(
                    "No objects is defined in document that was passed. At least one object must be defined.");
            }

            ProcessingDocument = document;

            var result = Stream.Null;
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
                    result = _module.GetOutput(converterPtr);
                }
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

            return result;
        }
    }
}
