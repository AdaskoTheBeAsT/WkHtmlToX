using System;
using System.Diagnostics.CodeAnalysis;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Native;
using AdaskoTheBeAsT.WkHtmlToX.Utils;

namespace AdaskoTheBeAsT.WkHtmlToX.Modules
{
    [ExcludeFromCodeCoverage]
    internal class WkHtmlToImageWindowsCommonModule
        : WkHtmlToXModule
    {
        internal WkHtmlToImageWindowsCommonModule()
        {
        }

        internal WkHtmlToImageWindowsCommonModule(IBufferManager bufferManager)
            : base(bufferManager)
        {
        }

        public override int Initialize(
            int useGraphics) =>
            NativeMethodsImageWindows.Initialize(useGraphics);

        public override int Terminate() => NativeMethodsImageWindows.Terminate();

        public override int ExtendedQt() => NativeMethodsImageWindows.ExtendedQt();

        public override IntPtr CreateGlobalSettings() => NativeMethodsImageWindows.CreateGlobalSettings();

        public override int DestroyGlobalSetting(
            IntPtr settings) =>
            NativeMethodsImageWindows.DestroyGlobalSettings(settings);

        public override int SetGlobalSetting(
            IntPtr settings,
            string name,
            string? value) =>
            NativeMethodsImageWindows.SetGlobalSettings(
                settings,
                name,
                value);

        public override IntPtr CreateConverter(
            IntPtr globalSettings) =>
            NativeMethodsImageWindows.CreateConverter(globalSettings);

        public override void DestroyConverter(
            IntPtr converter) =>
            NativeMethodsImageWindows.DestroyConverter(converter);

        public override int SetWarningCallback(
            IntPtr converter,
            StringCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsImageWindows.SetWarningCallback(
                converter,
                callback);
        }

        public override int SetErrorCallback(
            IntPtr converter,
            StringCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsImageWindows.SetErrorCallback(
                converter,
                callback);
        }

        public override int SetPhaseChangedCallback(
            IntPtr converter,
            VoidCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsImageWindows.SetPhaseChangedCallback(
                converter,
                callback);
        }

        public override int SetProgressChangedCallback(
            IntPtr converter,
            VoidCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsImageWindows.SetProgressChangedCallback(
                converter,
                callback);
        }

        public override int SetFinishedCallback(
            IntPtr converter,
            IntCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsImageWindows.SetFinishedCallback(
                converter,
                callback);
        }

        public override bool Convert(
            IntPtr converter) =>
            NativeMethodsImageWindows.Convert(converter);

        public override int GetCurrentPhase(
            IntPtr converter) =>
            NativeMethodsImageWindows.GetCurrentPhase(
                converter);

        public override int GetPhaseCount(
            IntPtr converter) =>
            NativeMethodsImageWindows.GetPhaseCount(
                converter);

        public override int GetHttpErrorCode(
            IntPtr converter) =>
            NativeMethodsImageWindows.GetHttpErrorCode(
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

            return NativeMethodsImageWindows.GetGlobalSettings(
                settings,
                name,
                buffer,
                buffer.Length);
        }

        protected override int GetOutputImpl(
            IntPtr converter,
            out IntPtr data) =>
            NativeMethodsImageWindows.GetOutput(
                converter,
                out data);

        protected override IntPtr GetLibraryVersionImpl() =>
            NativeMethodsImageWindows.GetVersion();

        protected override IntPtr GetPhaseDescriptionImpl(
            IntPtr converter,
            int phase) =>
            NativeMethodsImageWindows.GetPhaseDescription(
                converter,
                phase);

        protected override IntPtr GetProgressStringImpl(
            IntPtr converter) =>
            NativeMethodsImageWindows.GetProgressDescription(
                converter);
    }
}
