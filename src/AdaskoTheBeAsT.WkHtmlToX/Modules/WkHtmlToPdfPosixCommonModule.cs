using System;
using System.Diagnostics.CodeAnalysis;
using AdaskoTheBeAsT.WkHtmlToX.Native;
using AdaskoTheBeAsT.WkHtmlToX.Utils;

namespace AdaskoTheBeAsT.WkHtmlToX.Modules
{
    [ExcludeFromCodeCoverage]
    internal class WkHtmlToPdfPosixCommonModule
        : WkHtmlToXModule
    {
        public override int Initialize(
            int useGraphics) =>
            NativeMethodsPdfPosix.Initialize(useGraphics);

        public override int Terminate() => NativeMethodsPdfPosix.Terminate();

        public override int ExtendedQt() => NativeMethodsPdfPosix.ExtendedQt();

        public override IntPtr CreateGlobalSettings() => NativeMethodsPdfPosix.CreateGlobalSettings();

        public override int DestroyGlobalSetting(
            IntPtr settings) =>
            NativeMethodsPdfPosix.DestroyGlobalSettings(settings);

        public override int SetGlobalSetting(
            IntPtr settings,
            string name,
            string? value) => NativeMethodsPdfPosix.SetGlobalSettings(settings, name, value);

        public override IntPtr CreateConverter(
            IntPtr globalSettings) =>
            NativeMethodsPdfPosix.CreateConverter(globalSettings);

        public override void DestroyConverter(
            IntPtr converter) =>
            NativeMethodsPdfPosix.DestroyConverter(converter);

        public override int SetWarningCallback(
            IntPtr converter,
            StringCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsPdfPosix.SetWarningCallback(converter, callback);
        }

        public override int SetErrorCallback(
            IntPtr converter,
            StringCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsPdfPosix.SetErrorCallback(converter, callback);
        }

        public override int SetPhaseChangedCallback(
            IntPtr converter,
            VoidCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsPdfPosix.SetPhaseChangedCallback(converter, callback);
        }

        public override int SetProgressChangedCallback(
            IntPtr converter,
            VoidCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsPdfPosix.SetProgressChangedCallback(converter, callback);
        }

        public override int SetFinishedCallback(
            IntPtr converter,
            IntCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsPdfPosix.SetFinishedCallback(converter, callback);
        }

        public override bool Convert(
            IntPtr converter) =>
            NativeMethodsPdfPosix.Convert(converter);

        public override int GetCurrentPhase(
            IntPtr converter) =>
            NativeMethodsPdfPosix.GetCurrentPhase(converter);

        public override int GetPhaseCount(
            IntPtr converter) =>
            NativeMethodsPdfPosix.GetPhaseCount(converter);

        public override int GetHttpErrorCode(
            IntPtr converter) =>
            NativeMethodsPdfPosix.GetHttpErrorCode(converter);

        protected override int GetGlobalSettingImpl(
            IntPtr settings,
            string name,
            byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            return NativeMethodsPdfPosix.GetGlobalSettings(
                settings,
                name,
                buffer,
                buffer.Length);
        }

        protected override int GetOutputImpl(
            IntPtr converter,
            out IntPtr data) =>
            NativeMethodsPdfPosix.GetOutput(converter, out data);

        protected override IntPtr GetLibraryVersionImpl() => NativeMethodsPdfPosix.GetVersion();

        protected override IntPtr GetPhaseDescriptionImpl(
            IntPtr converter,
            int phase) =>
            NativeMethodsPdfPosix.GetPhaseDescription(converter, phase);

        protected override IntPtr GetProgressStringImpl(
            IntPtr converter) =>
            NativeMethodsPdfPosix.GetProgressDescription(converter);
    }
}
