using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Exceptions;
using AdaskoTheBeAsT.WkHtmlToX.Native;
using AdaskoTheBeAsT.WkHtmlToX.Utils;

namespace AdaskoTheBeAsT.WkHtmlToX.Modules;

[ExcludeFromCodeCoverage]
internal sealed class WkHtmlToPdfModule
    : WkHtmlToXModule,
        IWkHtmlToPdfModule
{
    public override int Initialize(
        int useGraphics) =>
        PdfNativeMethods.wkhtmltopdf_init(useGraphics);

    public override int Terminate() => PdfNativeMethods.wkhtmltopdf_deinit();

    public override int ExtendedQt() => PdfNativeMethods.wkhtmltopdf_extended_qt();

    public override IntPtr CreateGlobalSettings() => PdfNativeMethods.wkhtmltopdf_create_global_settings();

    public override int DestroyGlobalSetting(
        IntPtr settings) =>
        PdfNativeMethods.wkhtmltopdf_destroy_global_settings(settings);

    public override int SetGlobalSetting(
        IntPtr settings,
        string name,
        string? value) => PdfNativeMethods.wkhtmltopdf_set_global_setting(settings, name, value);

    public override IntPtr CreateConverter(
        IntPtr globalSettings) =>
        PdfNativeMethods.wkhtmltopdf_create_converter(globalSettings);

    public override void DestroyConverter(
        IntPtr converter) =>
        PdfNativeMethods.wkhtmltopdf_destroy_converter(converter);

    public override int SetWarningCallback(
        IntPtr converter,
        StringCallback callback)
    {
        return PdfNativeMethods.wkhtmltopdf_set_warning_callback(converter, callback);
    }

    public override int SetErrorCallback(
        IntPtr converter,
        StringCallback callback)
    {
        return PdfNativeMethods.wkhtmltopdf_set_error_callback(converter, callback);
    }

    public override int SetPhaseChangedCallback(
        IntPtr converter,
        VoidCallback callback)
    {
        return PdfNativeMethods.wkhtmltopdf_set_phase_changed_callback(converter, callback);
    }

    public override int SetProgressChangedCallback(
        IntPtr converter,
        VoidCallback callback)
    {
        return PdfNativeMethods.wkhtmltopdf_set_progress_changed_callback(converter, callback);
    }

    public override int SetFinishedCallback(
        IntPtr converter,
        IntCallback callback)
    {
        return PdfNativeMethods.wkhtmltopdf_set_finished_callback(converter, callback);
    }

    public override bool Convert(
        IntPtr converter) =>
        PdfNativeMethods.wkhtmltopdf_convert(converter);

    public override int GetCurrentPhase(
        IntPtr converter) =>
        PdfNativeMethods.wkhtmltopdf_current_phase(converter);

    public override int GetPhaseCount(
        IntPtr converter) =>
        PdfNativeMethods.wkhtmltopdf_phase_count(converter);

    public override int GetHttpErrorCode(
        IntPtr converter) =>
        PdfNativeMethods.wkhtmltopdf_http_error_code(converter);

    public IntPtr CreateObjectSettings() => PdfNativeMethods.wkhtmltopdf_create_object_settings();

    public int DestroyObjectSetting(
        IntPtr settings) =>
        PdfNativeMethods.wkhtmltopdf_destroy_object_settings(settings);

    public int SetObjectSetting(
        IntPtr settings,
        string name,
        string? value) => PdfNativeMethods.wkhtmltopdf_set_object_setting(settings, name, value);

    public string GetObjectSetting(
        IntPtr settings,
        string name)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(MaxBufferSize);
        try
        {
            var retVal = PdfNativeMethods.wkhtmltopdf_get_object_setting(
                settings,
                name,
                buffer,
                buffer.Length);

            if (retVal != 1)
            {
                throw new GetObjectSettingsFailedException($"GetObjectSettings failed for obtaining setting={name}");
            }

            var nullPos = Array.IndexOf(buffer, byte.MinValue);

            return Encoding.UTF8.GetString(buffer, 0, nullPos);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public void AddObject(
        IntPtr converter,
        IntPtr objectSettings,
        byte[] data) =>
        PdfNativeMethods.wkhtmltopdf_add_object(converter, objectSettings, data);

    public void AddObject(
        IntPtr converter,
        IntPtr objectSettings,
        string data) =>
        PdfNativeMethods.wkhtmltopdf_add_object(converter, objectSettings, data);

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
        return PdfNativeMethods.wkhtmltopdf_get_global_setting(
            settings,
            name,
            buffer,
            buffer.Length);
    }

    protected override int GetOutputImpl(
        IntPtr converter,
        out IntPtr data) =>
        PdfNativeMethods.wkhtmltopdf_get_output(converter, out data);

    protected override IntPtr GetLibraryVersionImpl() => PdfNativeMethods.wkhtmltopdf_version();

    protected override IntPtr GetPhaseDescriptionImpl(
        IntPtr converter,
        int phase) =>
        PdfNativeMethods.wkhtmltopdf_phase_description(converter, phase);

    protected override IntPtr GetProgressStringImpl(
        IntPtr converter) =>
        PdfNativeMethods.wkhtmltopdf_progress_string(converter);
}
