using System;
using System.Diagnostics.CodeAnalysis;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Native;
using AdaskoTheBeAsT.WkHtmlToX.Utils;

namespace AdaskoTheBeAsT.WkHtmlToX.Modules;

[ExcludeFromCodeCoverage]
internal sealed class WkHtmlToImageModule
    : WkHtmlToXModule,
        IWkHtmlToImageModule
{
    public override int Initialize(
        int useGraphics) =>
        ImageNativeMethods.wkhtmltoimage_init(useGraphics);

    public override int Terminate() => ImageNativeMethods.wkhtmltoimage_deinit();

    public override int ExtendedQt() => ImageNativeMethods.wkhtmltoimage_extended_qt();

    public override IntPtr CreateGlobalSettings() => ImageNativeMethods.wkhtmltoimage_create_global_settings();

    public override int DestroyGlobalSetting(
        IntPtr settings) =>
        ImageNativeMethods.wkhtmltoimage_destroy_global_settings(settings);

    public override int SetGlobalSetting(
        IntPtr settings,
        string name,
        string? value) =>
        ImageNativeMethods.wkhtmltoimage_set_global_setting(
            settings,
            name,
            value);

    public override IntPtr CreateConverter(
        IntPtr globalSettings) =>
        ImageNativeMethods.wkhtmltoimage_create_converter(globalSettings, IntPtr.Zero);

    public override void DestroyConverter(
        IntPtr converter) =>
        ImageNativeMethods.wkhtmltoimage_destroy_converter(converter);

    public override int SetWarningCallback(
        IntPtr converter,
        StringCallback callback)
    {
        return ImageNativeMethods.wkhtmltoimage_set_warning_callback(
            converter,
            callback);
    }

    public override int SetErrorCallback(
        IntPtr converter,
        StringCallback callback)
    {
        return ImageNativeMethods.wkhtmltoimage_set_error_callback(
            converter,
            callback);
    }

    public override int SetPhaseChangedCallback(
        IntPtr converter,
        VoidCallback callback)
    {
        return ImageNativeMethods.wkhtmltoimage_set_phase_changed_callback(
            converter,
            callback);
    }

    public override int SetProgressChangedCallback(
        IntPtr converter,
        VoidCallback callback)
    {
        return ImageNativeMethods.wkhtmltoimage_set_progress_changed_callback(
            converter,
            callback);
    }

    public override int SetFinishedCallback(
        IntPtr converter,
        IntCallback callback)
    {
        return ImageNativeMethods.wkhtmltoimage_set_finished_callback(
            converter,
            callback);
    }

    public override bool Convert(
        IntPtr converter) =>
        ImageNativeMethods.wkhtmltoimage_convert(converter);

    public override int GetCurrentPhase(
        IntPtr converter) =>
        ImageNativeMethods.wkhtmltoimage_current_phase(
            converter);

    public override int GetPhaseCount(
        IntPtr converter) =>
        ImageNativeMethods.wkhtmltoimage_phase_count(
            converter);

    public override int GetHttpErrorCode(
        IntPtr converter) =>
        ImageNativeMethods.wkhtmltoimage_http_error_code(
            converter);

    protected override int GetGlobalSettingImpl(
        IntPtr settings,
        string name,
        byte[] buffer)
    {
#if NETSTANDARD2_0
        if (buffer is null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }
#endif
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(buffer);
#endif
        return ImageNativeMethods.wkhtmltoimage_get_global_setting(
            settings,
            name,
            buffer,
            buffer.Length);
    }

    protected override int GetOutputImpl(
        IntPtr converter,
        out IntPtr data) =>
        ImageNativeMethods.wkhtmltoimage_get_output(
            converter,
            out data);

    protected override IntPtr GetLibraryVersionImpl() =>
        ImageNativeMethods.wkhtmltoimage_version();

    protected override IntPtr GetPhaseDescriptionImpl(
        IntPtr converter,
        int phase) =>
        ImageNativeMethods.wkhtmltoimage_phase_description(
            converter,
            phase);

    protected override IntPtr GetProgressStringImpl(
        IntPtr converter) =>
        ImageNativeMethods.wkhtmltoimage_progress_string(
            converter);
}
