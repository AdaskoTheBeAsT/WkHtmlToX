using System;
using AdaskoTheBeAsT.WkHtmlToX.Native;
using AdaskoTheBeAsT.WkHtmlToX.Utils;

namespace AdaskoTheBeAsT.WkHtmlToX.Modules
{
    internal class WkHtmlToPdfWindowsCommonModule
        : WkHtmlToXModule
    {
        public override int Initialize(
            int useGraphics) =>
            NativeMethodsPdfWindows.Initialize(useGraphics);

        public override int Terminate() => NativeMethodsPdfWindows.Terminate();

        public override int ExtendedQt() => NativeMethodsPdfWindows.ExtendedQt();

        public override IntPtr CreateGlobalSettings() => NativeMethodsPdfWindows.CreateGlobalSettings();

        public override int DestroyGlobalSetting(
            IntPtr settings) =>
            NativeMethodsPdfWindows.DestroyGlobalSettings(settings);

        public override int SetGlobalSetting(
            IntPtr settings,
            string name,
            string? value) => NativeMethodsPdfWindows.SetGlobalSettings(settings, name, value);

        public override IntPtr CreateConverter(
            IntPtr globalSettings) =>
            NativeMethodsPdfWindows.CreateConverter(globalSettings);

        public override void DestroyConverter(
            IntPtr converter) =>
            NativeMethodsPdfWindows.DestroyConverter(converter);

        public override int SetWarningCallback(
            IntPtr converter,
            StringCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsPdfWindows.SetWarningCallback(converter, callback);
        }

        public override int SetErrorCallback(
            IntPtr converter,
            StringCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsPdfWindows.SetErrorCallback(converter, callback);
        }

        public override int SetPhaseChangedCallback(
            IntPtr converter,
            VoidCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsPdfWindows.SetPhaseChangedCallback(converter, callback);
        }

        public override int SetProgressChangedCallback(
            IntPtr converter,
            VoidCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsPdfWindows.SetProgressChangedCallback(converter, callback);
        }

        public override int SetFinishedCallback(
            IntPtr converter,
            IntCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsPdfWindows.SetFinishedCallback(converter, callback);
        }

        public override bool Convert(
            IntPtr converter) =>
            NativeMethodsPdfWindows.Convert(converter);

        public override int GetCurrentPhase(
            IntPtr converter) =>
            NativeMethodsPdfWindows.GetCurrentPhase(converter);

        public override int GetPhaseCount(
            IntPtr converter) =>
            NativeMethodsPdfWindows.GetPhaseCount(converter);

        public override int GetHttpErrorCode(
            IntPtr converter) =>
            NativeMethodsPdfWindows.GetHttpErrorCode(converter);

        protected override int GetGlobalSettingImpl(
            IntPtr settings,
            string name,
            byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            return NativeMethodsPdfWindows.GetGlobalSettings(
                settings,
                name,
                buffer,
                buffer.Length);
        }

        protected override int GetOutputImpl(
            IntPtr converter,
            out IntPtr data) =>
            NativeMethodsPdfWindows.GetOutput(converter, out data);

        protected override IntPtr GetLibraryVersionImpl() => NativeMethodsPdfWindows.GetVersion();

        protected override IntPtr GetPhaseDescriptionImpl(
            IntPtr converter,
            int phase) =>
            NativeMethodsPdfWindows.GetPhaseDescription(converter, phase);

        protected override IntPtr GetProgressStringImpl(
            IntPtr converter) =>
            NativeMethodsPdfWindows.GetProgressDescription(converter);
    }
}
