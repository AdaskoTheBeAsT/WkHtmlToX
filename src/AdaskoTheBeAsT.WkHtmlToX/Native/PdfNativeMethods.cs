#nullable enable
using System;
using System.Runtime.InteropServices;
using System.Security;
using AdaskoTheBeAsT.WkHtmlToX.Utils;

namespace AdaskoTheBeAsT.WkHtmlToX.Native
{
#pragma warning disable CA1060 // Move pinvokes to native methods class
#pragma warning disable CA2101 // Specify marshaling for P/Invoke string arguments
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable SA1300 // Element should begin with upper-case letter
    internal static class PdfNativeMethods
    {
        /// <summary>
        /// wkhtmltopdf_init.
        /// </summary>
        /// <param name="useGraphics"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern int wkhtmltopdf_init(int useGraphics);

        /// <summary>
        /// wkhtmltopdf_deinit.
        /// </summary>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern int wkhtmltopdf_deinit();

        /// <summary>
        /// wkhtmltopdf_extended_qt.
        /// </summary>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern int wkhtmltopdf_extended_qt();

        /// <summary>
        /// wkhtmltopdf_version.
        /// </summary>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern IntPtr wkhtmltopdf_version();

        /// <summary>
        /// wkhtmltopdf_create_global_settings.
        /// </summary>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern IntPtr wkhtmltopdf_create_global_settings();

        /// <summary>
        /// wkhtmltopdf_destroy_global_settings.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern int wkhtmltopdf_destroy_global_settings(IntPtr settings);

        /// <summary>
        /// wkhtmltopdf_create_object_settings.
        /// </summary>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern IntPtr wkhtmltopdf_create_object_settings();

        /// <summary>
        /// wkhtmltopdf_destroy_object_settings.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern int wkhtmltopdf_destroy_object_settings(IntPtr settings);

        /// <summary>
        /// wkhtmltopdf_set_global_setting.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern int wkhtmltopdf_set_global_setting(
            IntPtr settings,
            [MarshalAs((short)CustomUnmanagedType.LPUTF8Str)] string name,
            [MarshalAs((short)CustomUnmanagedType.LPUTF8Str)] string? value);

        /// <summary>
        /// wkhtmltopdf_get_global_setting.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="valueSize"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern int wkhtmltopdf_get_global_setting(
            IntPtr settings,
            [MarshalAs((short)CustomUnmanagedType.LPUTF8Str)] string name,
            [MarshalAs(UnmanagedType.LPArray)] byte[] value,
            int valueSize);

        /// <summary>
        /// wkhtmltopdf_set_object_setting.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern int wkhtmltopdf_set_object_setting(
            IntPtr settings,
            [MarshalAs((short)CustomUnmanagedType.LPUTF8Str)] string name,
            [MarshalAs((short)CustomUnmanagedType.LPUTF8Str)] string? value);

        /// <summary>
        /// wkhtmltopdf_get_object_setting.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="valueSize"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern int wkhtmltopdf_get_object_setting(
            IntPtr settings,
            [MarshalAs((short)CustomUnmanagedType.LPUTF8Str)] string name,
            [MarshalAs(UnmanagedType.LPArray)] byte[] value,
            int valueSize);

        /// <summary>
        /// wkhtmltopdf_create_converter.
        /// </summary>
        /// <param name="globalSettings"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern IntPtr wkhtmltopdf_create_converter(IntPtr globalSettings);

        /// <summary>
        /// wkhtmltopdf_destroy_converter.
        /// </summary>
        /// <param name="converter"></param>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern void wkhtmltopdf_destroy_converter(IntPtr converter);

        /// <summary>
        /// wkhtmltopdf_set_warning_callback.
        /// </summary>
        /// <param name="converter"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern int wkhtmltopdf_set_warning_callback(
            IntPtr converter,
            [MarshalAs(UnmanagedType.FunctionPtr)] StringCallback callback);

        /// <summary>
        /// wkhtmltopdf_set_error_callback.
        /// </summary>
        /// <param name="converter"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern int wkhtmltopdf_set_error_callback(
            IntPtr converter,
            [MarshalAs(UnmanagedType.FunctionPtr)] StringCallback callback);

        /// <summary>
        /// wkhtmltopdf_set_phase_changed_callback.
        /// </summary>
        /// <param name="converter"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern int wkhtmltopdf_set_phase_changed_callback(
            IntPtr converter,
            [MarshalAs(UnmanagedType.FunctionPtr)] VoidCallback callback);

        /// <summary>
        /// wkhtmltopdf_set_progress_changed_callback.
        /// </summary>
        /// <param name="converter"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern int wkhtmltopdf_set_progress_changed_callback(
            IntPtr converter,
            [MarshalAs(UnmanagedType.FunctionPtr)] VoidCallback callback);

        /// <summary>
        /// wkhtmltopdf_set_finished_callback.
        /// </summary>
        /// <param name="converter"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern int wkhtmltopdf_set_finished_callback(
            IntPtr converter,
            [MarshalAs(UnmanagedType.FunctionPtr)] IntCallback callback);

        /// <summary>
        /// wkhtmltopdf_convert.
        /// </summary>
        /// <param name="converter"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern bool wkhtmltopdf_convert(IntPtr converter);

        /// <summary>
        /// wkhtmltopdf_add_object.
        /// </summary>
        /// <param name="converter"></param>
        /// <param name="objectSettings"></param>
        /// <param name="data"></param>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern void wkhtmltopdf_add_object(
            IntPtr converter,
            IntPtr objectSettings,
            byte[] data);

        /// <summary>
        /// wkhtmltopdf_add_object.
        /// </summary>
        /// <param name="converter"></param>
        /// <param name="objectSettings"></param>
        /// <param name="data"></param>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern void wkhtmltopdf_add_object(
            IntPtr converter,
            IntPtr objectSettings,
            [MarshalAs((short)CustomUnmanagedType.LPUTF8Str)] string data);

        /// <summary>
        /// wkhtmltopdf_current_phase.
        /// </summary>
        /// <param name="converter"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern int wkhtmltopdf_current_phase(IntPtr converter);

        /// <summary>
        /// wkhtmltopdf_phase_description.
        /// </summary>
        /// <param name="converter"></param>
        /// <param name="phase"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern IntPtr wkhtmltopdf_phase_description(IntPtr converter, int phase);

        /// <summary>
        /// wkhtmltopdf_progress_string.
        /// </summary>
        /// <param name="converter"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern IntPtr wkhtmltopdf_progress_string(IntPtr converter);

        /// <summary>
        /// wkhtmltopdf_phase_count.
        /// </summary>
        /// <param name="converter"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern int wkhtmltopdf_phase_count(IntPtr converter);

        /// <summary>
        /// wkhtmltopdf_http_error_code.
        /// </summary>
        /// <param name="converter"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern int wkhtmltopdf_http_error_code(IntPtr converter);

        /// <summary>
        /// wkhtmltopdf_get_output.
        /// </summary>
        /// <param name="converter"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern int wkhtmltopdf_get_output(IntPtr converter, out IntPtr data);
    }
#pragma warning restore SA1300 // Element should begin with upper-case letter
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CA2101 // Specify marshaling for P/Invoke string arguments
#pragma warning restore CA1060 // Move pinvokes to native methods class
}
