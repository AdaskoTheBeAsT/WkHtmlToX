using System;
using System.IO;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Utils;

namespace AdaskoTheBeAsT.WkHtmlToX.Engine
{
    internal class ImageProcessor
        : ProcessorBase,
            IImageProcessor
    {
        private readonly IWkHtmlToImageModule _wkHtmlToImageModule;

        public ImageProcessor(WkHtmlToXConfiguration configuration, IWkHtmlToImageModule wkHtmlToImageModule)
            : base(configuration)
        {
            _wkHtmlToImageModule = wkHtmlToImageModule ?? throw new ArgumentNullException(nameof(wkHtmlToImageModule));
        }

        public IWkHtmlToImageModule WkHtmlToImageModule => _wkHtmlToImageModule;

        public bool Convert(IHtmlToImageDocument? document, Func<int, Stream> createStreamFunc)
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

            var (converterPtr, globalSettingsPtr) = CreateConverter(document);

            RegisterEvents(converterPtr);

            try
            {
                var converted = _wkHtmlToImageModule.Convert(converterPtr);

                if (converted)
                {
                    _wkHtmlToImageModule.GetOutput(converterPtr, createStreamFunc);
                }

                return converted;
            }
            finally
            {
                _wkHtmlToImageModule.DestroyGlobalSetting(globalSettingsPtr);
                _wkHtmlToImageModule.DestroyConverter(converterPtr);
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

            var globalSettings = _wkHtmlToImageModule.CreateGlobalSettings();
            ApplyConfig(globalSettings, document.ImageSettings, isGlobal: true);
            var converter = _wkHtmlToImageModule.CreateConverter(globalSettings);

            return (converter, globalSettings);
        }

        protected internal override Func<IntPtr, string, string?, int> GetApplySettingFunc(bool isGlobal) =>
            _wkHtmlToImageModule.SetGlobalSetting;

        protected internal override int GetCurrentPhase(IntPtr converter) => _wkHtmlToImageModule.GetCurrentPhase(converter);

        protected internal override int GetPhaseCount(IntPtr converter) => _wkHtmlToImageModule.GetPhaseCount(converter);

        protected internal override string GetPhaseDescription(
            IntPtr converter,
            int phase) =>
            _wkHtmlToImageModule.GetPhaseDescription(converter, phase);

        protected internal override string GetProgressDescription(IntPtr converter) =>
            _wkHtmlToImageModule.GetProgressDescription(converter);

        protected internal override int SetWarningCallback(
            IntPtr converter,
            StringCallback callback) =>
            _wkHtmlToImageModule.SetWarningCallback(converter, callback);

        protected internal override int SetErrorCallback(
            IntPtr converter,
            StringCallback callback) =>
            _wkHtmlToImageModule.SetErrorCallback(converter, callback);

        protected internal override int SetPhaseChangedCallback(
            IntPtr converter,
            VoidCallback callback) =>
            _wkHtmlToImageModule.SetPhaseChangedCallback(converter, callback);

        protected internal override int SetProgressChangedCallback(
            IntPtr converter,
            VoidCallback callback) =>
            _wkHtmlToImageModule.SetProgressChangedCallback(converter, callback);

        protected internal override int SetFinishedCallback(
            IntPtr converter,
            IntCallback callback) =>
            _wkHtmlToImageModule.SetFinishedCallback(converter, callback);
    }
}
