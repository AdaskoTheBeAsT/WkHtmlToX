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

        public bool Convert(IHtmlToImageDocument document, Func<int, Stream> createStreamFunc)
        {
            if (document is null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (createStreamFunc is null)
            {
                throw new ArgumentNullException(nameof(createStreamFunc));
            }

            var converted = false;
            var thread = new Thread(
                () => converted = ConvertImpl(document, createStreamFunc))
            {
                IsBackground = true,
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            return converted;
        }

        internal bool ConvertImpl(IHtmlToImageDocument document, Func<int, Stream> createStreamFunc)
        {
            if (document?.ImageSettings == null)
            {
                throw new ArgumentException(
                    "No image settings is defined in document that was passed. At least one object must be defined.");
            }

            if (createStreamFunc is null)
            {
                throw new ArgumentNullException(nameof(createStreamFunc));
            }

            ProcessingDocument = document;

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
                    _module.GetOutput(converterPtr, createStreamFunc);
                }

                return converted;
            }
            finally
            {
                _module.DestroyGlobalSetting(globalSettingsPtr);
                _module.DestroyConverter(converterPtr);
                _module.Terminate();
                ProcessingDocument = null;
            }
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
