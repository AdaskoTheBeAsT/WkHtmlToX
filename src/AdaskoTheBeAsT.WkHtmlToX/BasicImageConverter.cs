using System;
using System.IO;
using System.Threading;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;

namespace AdaskoTheBeAsT.WkHtmlToX
{
    public class BasicImageConverter
        : ConverterBase,
            IHtmlToImageConverter
    {
        public BasicImageConverter()
            : base(ModuleKind.Image)
        {
        }

        internal BasicImageConverter(IWkHtmlToXModuleFactory moduleFactory)
            : base(moduleFactory, ModuleKind.Image)
        {
        }

        public Stream Convert(IHtmlToImageDocument document)
        {
            var pdfStream = Stream.Null;
            var thread = new Thread(
                () => pdfStream = ConvertImpl(document))
            {
                IsBackground = true,
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            return pdfStream;
        }

        internal Stream ConvertImpl(IHtmlToImageDocument document)
        {
            if (document?.ImageSettings == null)
            {
                throw new ArgumentException(
                    "No image settings is defined in document that was passed. At least one object must be defined.");
            }

            ProcessingDocument = document;

            var result = Stream.Null;
            var loaded = _module.Initialize(0) == 1;
            if (!loaded)
            {
                throw new ArgumentException("Not loaded");
            }

            var (converterPtr, globalSettingsPtr) = CreateConverter(document);

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
                _module.DestroyGlobalSetting(globalSettingsPtr);
                _module.DestroyConverter(converterPtr);
                _module.Terminate();
                ProcessingDocument = null;
            }

            return result;
        }

        internal (IntPtr converterPtr, IntPtr globalSettingsPtr) CreateConverter(
            IHtmlToImageDocument document)
        {
            if (document is null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var globalSettings = _module.CreateGlobalSettings();
            ApplyConfig(globalSettings, document.ImageSettings, true);
            var converter = _module.CreateConverter(globalSettings);

            return (converter, globalSettings);
        }
    }
}
