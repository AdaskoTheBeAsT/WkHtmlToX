using System;
using System.IO;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Utils;

namespace AdaskoTheBeAsT.WkHtmlToX.Engine
{
    internal sealed class ImageProcessor
        : ProcessorBase,
            IImageProcessor
    {
        public ImageProcessor(WkHtmlToXConfiguration configuration, IWkHtmlToImageModule wkHtmlToImageModule)
            : base(configuration)
        {
            WkHtmlToImageModule = wkHtmlToImageModule ?? throw new ArgumentNullException(nameof(wkHtmlToImageModule));
        }

        public IWkHtmlToImageModule WkHtmlToImageModule { get; }

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
                var converted = WkHtmlToImageModule.Convert(converterPtr);

                if (converted)
                {
                    WkHtmlToImageModule.GetOutput(converterPtr, createStreamFunc);
                }

                return converted;
            }
            finally
            {
                WkHtmlToImageModule.DestroyGlobalSetting(globalSettingsPtr);
                WkHtmlToImageModule.DestroyConverter(converterPtr);
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

            var globalSettings = WkHtmlToImageModule.CreateGlobalSettings();
            ApplyConfig(globalSettings, document.ImageSettings, isGlobal: true);
            var converter = WkHtmlToImageModule.CreateConverter(globalSettings);

            return (converter, globalSettings);
        }

        protected internal override Func<IntPtr, string, string?, int> GetApplySettingFunc(bool isGlobal) =>
            WkHtmlToImageModule.SetGlobalSetting;

        protected internal override int GetCurrentPhase(IntPtr converter) => WkHtmlToImageModule.GetCurrentPhase(converter);

        protected internal override int GetPhaseCount(IntPtr converter) => WkHtmlToImageModule.GetPhaseCount(converter);

        protected internal override string GetPhaseDescription(
            IntPtr converter,
            int phase) =>
            WkHtmlToImageModule.GetPhaseDescription(converter, phase);

        protected internal override string GetProgressDescription(IntPtr converter) =>
            WkHtmlToImageModule.GetProgressDescription(converter);

        protected internal override int SetWarningCallback(
            IntPtr converter,
            StringCallback callback) =>
            WkHtmlToImageModule.SetWarningCallback(converter, callback);

        protected internal override int SetErrorCallback(
            IntPtr converter,
            StringCallback callback) =>
            WkHtmlToImageModule.SetErrorCallback(converter, callback);

        protected internal override int SetPhaseChangedCallback(
            IntPtr converter,
            VoidCallback callback) =>
            WkHtmlToImageModule.SetPhaseChangedCallback(converter, callback);

        protected internal override int SetProgressChangedCallback(
            IntPtr converter,
            VoidCallback callback) =>
            WkHtmlToImageModule.SetProgressChangedCallback(converter, callback);

        protected internal override int SetFinishedCallback(
            IntPtr converter,
            IntCallback callback) =>
            WkHtmlToImageModule.SetFinishedCallback(converter, callback);
    }
}
