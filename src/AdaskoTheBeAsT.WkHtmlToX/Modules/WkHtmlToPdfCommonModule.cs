#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using AdaskoTheBeAsT.WkHtmlToX.Native;
using AdaskoTheBeAsT.WkHtmlToX.Utils;

namespace AdaskoTheBeAsT.WkHtmlToX.Modules
{
    [ExcludeFromCodeCoverage]
    internal sealed class WkHtmlToPdfCommonModule
        : WkHtmlToXModule
    {
        public override int Initialize(
            int useGraphics) =>
            NativeMethodsPdf.wkhtmltopdf_init(useGraphics);

        public override int Terminate() => NativeMethodsPdf.wkhtmltopdf_deinit();

        public override int ExtendedQt() => NativeMethodsPdf.wkhtmltopdf_extended_qt();

        public override IntPtr CreateGlobalSettings() => NativeMethodsPdf.wkhtmltopdf_create_global_settings();

        public override int DestroyGlobalSetting(
            IntPtr settings) =>
            NativeMethodsPdf.wkhtmltopdf_destroy_global_settings(settings);

        public override int SetGlobalSetting(
            IntPtr settings,
            string name,
            string? value) => NativeMethodsPdf.wkhtmltopdf_set_global_setting(settings, name, value);

        public override IntPtr CreateConverter(
            IntPtr globalSettings) =>
            NativeMethodsPdf.wkhtmltopdf_create_converter(globalSettings);

        public override void DestroyConverter(
            IntPtr converter) =>
            NativeMethodsPdf.wkhtmltopdf_destroy_converter(converter);

        public override int SetWarningCallback(
            IntPtr converter,
            StringCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsPdf.wkhtmltopdf_set_warning_callback(converter, callback);
        }

        public override int SetErrorCallback(
            IntPtr converter,
            StringCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsPdf.wkhtmltopdf_set_error_callback(converter, callback);
        }

        public override int SetPhaseChangedCallback(
            IntPtr converter,
            VoidCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsPdf.wkhtmltopdf_set_phase_changed_callback(converter, callback);
        }

        public override int SetProgressChangedCallback(
            IntPtr converter,
            VoidCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsPdf.wkhtmltopdf_set_progress_changed_callback(converter, callback);
        }

        public override int SetFinishedCallback(
            IntPtr converter,
            IntCallback callback)
        {
            _delegates.Add(callback);
            return NativeMethodsPdf.wkhtmltopdf_set_finished_callback(converter, callback);
        }

        public override bool Convert(
            IntPtr converter) =>
            NativeMethodsPdf.wkhtmltopdf_convert(converter);

        public override int GetCurrentPhase(
            IntPtr converter) =>
            NativeMethodsPdf.wkhtmltopdf_current_phase(converter);

        public override int GetPhaseCount(
            IntPtr converter) =>
            NativeMethodsPdf.wkhtmltopdf_phase_count(converter);

        public override int GetHttpErrorCode(
            IntPtr converter) =>
            NativeMethodsPdf.wkhtmltopdf_http_error_code(converter);

        protected override int GetGlobalSettingImpl(
            IntPtr settings,
            string name,
            byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            return NativeMethodsPdf.wkhtmltopdf_get_global_setting(
                settings,
                name,
                buffer,
                buffer.Length);
        }

        protected override int GetOutputImpl(
            IntPtr converter,
            out IntPtr data) =>
            NativeMethodsPdf.wkhtmltopdf_get_output(converter, out data);

        protected override IntPtr GetLibraryVersionImpl() => NativeMethodsPdf.wkhtmltopdf_version();

        protected override IntPtr GetPhaseDescriptionImpl(
            IntPtr converter,
            int phase) =>
            NativeMethodsPdf.wkhtmltopdf_phase_description(converter, phase);

        protected override IntPtr GetProgressStringImpl(
            IntPtr converter) =>
            NativeMethodsPdf.wkhtmltopdf_progress_string(converter);
    }
}
