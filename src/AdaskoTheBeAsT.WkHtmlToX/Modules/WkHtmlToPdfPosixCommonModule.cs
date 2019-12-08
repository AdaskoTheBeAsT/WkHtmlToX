using System;
using AdaskoTheBeAsT.WkHtmlToX.Native;
using AdaskoTheBeAsT.WkHtmlToX.Utils;

namespace AdaskoTheBeAsT.WkHtmlToX.Modules
{
    internal class WkHtmlToPdfPosixCommonModule
        : WkHtmlToXModule
    {
        public override int Initialize(
            int useGraphics) =>
            NativeMethodsPdfPosix.wkhtmltopdf_init(useGraphics);

        public override int Terminate() => NativeMethodsPdfPosix.wkhtmltopdf_deinit();

        public override int ExtendedQt() => NativeMethodsPdfPosix.wkhtmltopdf_extended_qt();

        public override IntPtr CreateGlobalSettings() => NativeMethodsPdfPosix.wkhtmltopdf_create_global_settings();

        public override int DestroyGlobalSetting(
            IntPtr settings) =>
            NativeMethodsPdfPosix.wkhtmltopdf_destroy_global_settings(settings);

        public override int SetGlobalSetting(
            IntPtr settings,
            string name,
            string value) => NativeMethodsPdfPosix.wkhtmltopdf_set_global_setting(settings, name, value);

        public override IntPtr CreateConverter(
            IntPtr globalSettings) =>
            NativeMethodsPdfPosix.wkhtmltopdf_create_converter(globalSettings);

        public override void DestroyConverter(
            IntPtr converter) =>
            NativeMethodsPdfPosix.wkhtmltopdf_destroy_converter(converter);

        public override int SetWarningCallback(
            IntPtr converter,
            StringCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsPdfPosix.wkhtmltopdf_set_warning_callback(converter, callback);
        }

        public override int SetErrorCallback(
            IntPtr converter,
            StringCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsPdfPosix.wkhtmltopdf_set_error_callback(converter, callback);
        }

        public override int SetPhaseChangedCallback(
            IntPtr converter,
            VoidCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsPdfPosix.wkhtmltopdf_set_phase_changed_callback(converter, callback);
        }

        public override int SetProgressChangedCallback(
            IntPtr converter,
            VoidCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsPdfPosix.wkhtmltopdf_set_progress_changed_callback(converter, callback);
        }

        public override int SetFinishedCallback(
            IntPtr converter,
            IntCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsPdfPosix.wkhtmltopdf_set_finished_callback(converter, callback);
        }

        public override bool Convert(
            IntPtr converter) =>
            NativeMethodsPdfPosix.wkhtmltopdf_convert(converter);

        public override int GetCurrentPhase(
            IntPtr converter) =>
            NativeMethodsPdfPosix.wkhtmltopdf_current_phase(converter);

        public override int GetPhaseCount(
            IntPtr converter) =>
            NativeMethodsPdfPosix.wkhtmltopdf_phase_count(converter);

        public override int GetHttpErrorCode(
            IntPtr converter) =>
            NativeMethodsPdfPosix.wkhtmltopdf_http_error_code(converter);

        protected override int GetGlobalSettingImpl(
            IntPtr settings,
            string name,
            byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            return NativeMethodsPdfPosix.wkhtmltopdf_get_global_setting(
                settings,
                name,
                buffer,
                buffer.Length);
        }

        protected override int GetOutputImpl(
            IntPtr converter,
            out IntPtr data) =>
            NativeMethodsPdfPosix.wkhtmltopdf_get_output(converter, out data);

        protected override IntPtr GetLibraryVersionImpl() => NativeMethodsPdfPosix.wkhtmltopdf_version();

        protected override IntPtr GetPhaseDescriptionImpl(
            IntPtr converter,
            int phase) =>
            NativeMethodsPdfPosix.wkhtmltopdf_phase_description(converter, phase);

        protected override IntPtr GetProgressStringImpl(
            IntPtr converter) =>
            NativeMethodsPdfPosix.wkhtmltopdf_progress_string(converter);
    }
}
