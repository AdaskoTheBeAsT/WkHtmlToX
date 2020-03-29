using System;
using System.Diagnostics.CodeAnalysis;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Native;
using AdaskoTheBeAsT.WkHtmlToX.Utils;

namespace AdaskoTheBeAsT.WkHtmlToX.Modules
{
    [ExcludeFromCodeCoverage]
    internal class WkHtmlToImagePosixCommonModule
        : WkHtmlToXModule
    {
        internal WkHtmlToImagePosixCommonModule()
        {
        }

        internal WkHtmlToImagePosixCommonModule(IBufferManager bufferManager)
            : base(bufferManager)
        {
        }

        public override int Initialize(
            int useGraphics) =>
            NativeMethodsImagePosix.Initialize(useGraphics);

        public override int Terminate() => NativeMethodsImagePosix.Terminate();

        public override int ExtendedQt() => NativeMethodsImagePosix.ExtendedQt();

        public override IntPtr CreateGlobalSettings() => NativeMethodsImagePosix.CreateGlobalSettings();

        public override int DestroyGlobalSetting(
            IntPtr settings) =>
            NativeMethodsImagePosix.DestroyGlobalSettings(settings);

        public override int SetGlobalSetting(
            IntPtr settings,
            string name,
            string? value) =>
            NativeMethodsImagePosix.SetGlobalSettings(
                settings,
                name,
                value);

        public override IntPtr CreateConverter(
            IntPtr globalSettings) =>
            NativeMethodsImagePosix.CreateConverter(globalSettings);

        public override void DestroyConverter(
            IntPtr converter) =>
            NativeMethodsImagePosix.DestroyConverter(converter);

        public override int SetWarningCallback(
            IntPtr converter,
            StringCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsImagePosix.SetWarningCallback(
                converter,
                callback);
        }

        public override int SetErrorCallback(
            IntPtr converter,
            StringCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsImagePosix.SetErrorCallback(
                converter,
                callback);
        }

        public override int SetPhaseChangedCallback(
            IntPtr converter,
            VoidCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsImagePosix.SetPhaseChangedCallback(
                converter,
                callback);
        }

        public override int SetProgressChangedCallback(
            IntPtr converter,
            VoidCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsImagePosix.SetProgressChangedCallback(
                converter,
                callback);
        }

        public override int SetFinishedCallback(
            IntPtr converter,
            IntCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsImagePosix.SetFinishedCallback(
                converter,
                callback);
        }

        public override bool Convert(
            IntPtr converter) =>
            NativeMethodsImagePosix.Convert(converter);

        public override int GetCurrentPhase(
            IntPtr converter) =>
            NativeMethodsImagePosix.GetCurrentPhase(
                converter);

        public override int GetPhaseCount(
            IntPtr converter) =>
            NativeMethodsImagePosix.GetPhaseCount(
                converter);

        public override int GetHttpErrorCode(
            IntPtr converter) =>
            NativeMethodsImagePosix.GetHttpErrorCode(
                converter);

        protected override int GetGlobalSettingImpl(
            IntPtr settings,
            string name,
            byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            return NativeMethodsImagePosix.GetGlobalSettings(
                settings,
                name,
                buffer,
                buffer.Length);
        }

        protected override int GetOutputImpl(
            IntPtr converter,
            out IntPtr data) =>
            NativeMethodsImagePosix.GetOutput(
                converter,
                out data);

        protected override IntPtr GetLibraryVersionImpl() =>
            NativeMethodsImagePosix.GetVersion();

        protected override IntPtr GetPhaseDescriptionImpl(
            IntPtr converter,
            int phase) =>
            NativeMethodsImagePosix.GetPhaseDescription(
                converter,
                phase);

        protected override IntPtr GetProgressStringImpl(
            IntPtr converter) =>
            NativeMethodsImagePosix.GetProgressDescription(
                converter);
    }
}
