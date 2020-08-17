#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using AdaskoTheBeAsT.WkHtmlToX.Native;
using AdaskoTheBeAsT.WkHtmlToX.Utils;

namespace AdaskoTheBeAsT.WkHtmlToX.Modules
{
    [ExcludeFromCodeCoverage]
    internal sealed class WkHtmlToImageCommonModule
        : WkHtmlToXModule
    {
        public override int Initialize(
            int useGraphics) =>
            NativeMethodsImage.wkhtmltoimage_init(useGraphics);

        public override int Terminate() => NativeMethodsImage.wkhtmltoimage_deinit();

        public override int ExtendedQt() => NativeMethodsImage.wkhtmltoimage_extended_qt();

        public override IntPtr CreateGlobalSettings() => NativeMethodsImage.wkhtmltoimage_create_global_settings();

        public override int DestroyGlobalSetting(
            IntPtr settings) =>
            NativeMethodsImage.wkhtmltoimage_destroy_global_settings(settings);

        public override int SetGlobalSetting(
            IntPtr settings,
            string name,
            string? value) =>
            NativeMethodsImage.wkhtmltoimage_set_global_setting(
                settings,
                name,
                value);

        public override IntPtr CreateConverter(
            IntPtr globalSettings) =>
            NativeMethodsImage.wkhtmltoimage_create_converter(globalSettings);

        public override void DestroyConverter(
            IntPtr converter) =>
            NativeMethodsImage.wkhtmltoimage_destroy_converter(converter);

        public override int SetWarningCallback(
            IntPtr converter,
            StringCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsImage.wkhtmltoimage_set_warning_callback(
                converter,
                callback);
        }

        public override int SetErrorCallback(
            IntPtr converter,
            StringCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsImage.wkhtmltoimage_set_error_callback(
                converter,
                callback);
        }

        public override int SetPhaseChangedCallback(
            IntPtr converter,
            VoidCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsImage.wkhtmltoimage_set_phase_changed_callback(
                converter,
                callback);
        }

        public override int SetProgressChangedCallback(
            IntPtr converter,
            VoidCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsImage.wkhtmltoimage_set_progress_changed_callback(
                converter,
                callback);
        }

        public override int SetFinishedCallback(
            IntPtr converter,
            IntCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsImage.wkhtmltoimage_set_finished_callback(
                converter,
                callback);
        }

        public override bool Convert(
            IntPtr converter) =>
            NativeMethodsImage.wkhtmltoimage_convert(converter);

        public override int GetCurrentPhase(
            IntPtr converter) =>
            NativeMethodsImage.wkhtmltoimage_current_phase(
                converter);

        public override int GetPhaseCount(
            IntPtr converter) =>
            NativeMethodsImage.wkhtmltoimage_phase_count(
                converter);

        public override int GetHttpErrorCode(
            IntPtr converter) =>
            NativeMethodsImage.wkhtmltoimage_http_error_code(
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

            return NativeMethodsImage.wkhtmltoimage_get_global_setting(
                settings,
                name,
                buffer,
                buffer.Length);
        }

        protected override int GetOutputImpl(
            IntPtr converter,
            out IntPtr data) =>
            NativeMethodsImage.wkhtmltoimage_get_output(
                converter,
                out data);

        protected override IntPtr GetLibraryVersionImpl() =>
            NativeMethodsImage.wkhtmltoimage_version();

        protected override IntPtr GetPhaseDescriptionImpl(
            IntPtr converter,
            int phase) =>
            NativeMethodsImage.wkhtmltoimage_phase_description(
                converter,
                phase);

        protected override IntPtr GetProgressStringImpl(
            IntPtr converter) =>
            NativeMethodsImage.wkhtmltoimage_progress_string(
                converter);
    }
}
