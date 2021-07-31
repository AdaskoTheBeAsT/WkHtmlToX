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
    internal static class ImageNativeMethods
    {
        /// <summary>
        /// wkhtmltoimage_init.
        /// </summary>
        /// <param name="useGraphics"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]

        internal static extern int wkhtmltoimage_init(int useGraphics);

        /// <summary>
        /// wkhtmltoimage_deinit.
        /// </summary>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern int wkhtmltoimage_deinit();

        /// <summary>
        /// wkhtmltoimage_extended_qt.
        /// </summary>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern int wkhtmltoimage_extended_qt();

        /// <summary>
        /// wkhtmltoimage_version.
        /// </summary>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern IntPtr wkhtmltoimage_version();

        /// <summary>
        /// wkhtmltoimage_create_global_settings.
        /// </summary>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern IntPtr wkhtmltoimage_create_global_settings();

        /// <summary>
        /// wkhtmltoimage_destroy_global_settings.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern int wkhtmltoimage_destroy_global_settings(IntPtr settings);

        /// <summary>
        /// wkhtmltoimage_set_global_setting.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern int wkhtmltoimage_set_global_setting(
            IntPtr settings,
            [MarshalAs((short)CustomUnmanagedType.LPUTF8Str)] string name,
            [MarshalAs((short)CustomUnmanagedType.LPUTF8Str)] string? value);

        /// <summary>
        /// wkhtmltoimage_get_global_setting.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="valueSize"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern int wkhtmltoimage_get_global_setting(
            IntPtr settings,
            [MarshalAs((short)CustomUnmanagedType.LPUTF8Str)] string name,
            [MarshalAs(UnmanagedType.LPArray)] byte[] value,
            int valueSize);

        /// <summary>
        /// wkhtmltoimage_create_converter.
        /// </summary>
        /// <param name="globalSettings"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern IntPtr wkhtmltoimage_create_converter(IntPtr globalSettings);

        /// <summary>
        /// wkhtmltoimage_destroy_converter.
        /// </summary>
        /// <param name="converter"></param>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern void wkhtmltoimage_destroy_converter(IntPtr converter);

        /// <summary>
        /// wkhtmltoimage_set_warning_callback.
        /// </summary>
        /// <param name="converter"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern int wkhtmltoimage_set_warning_callback(
            IntPtr converter,
            [MarshalAs(UnmanagedType.FunctionPtr)] StringCallback callback);

        /// <summary>
        /// wkhtmltoimage_set_error_callback.
        /// </summary>
        /// <param name="converter"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern int wkhtmltoimage_set_error_callback(
            IntPtr converter,
            [MarshalAs(UnmanagedType.FunctionPtr)] StringCallback callback);

        /// <summary>
        /// wkhtmltoimage_set_phase_changed_callback.
        /// </summary>
        /// <param name="converter"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern int wkhtmltoimage_set_phase_changed_callback(
            IntPtr converter,
            [MarshalAs(UnmanagedType.FunctionPtr)] VoidCallback callback);

        /// <summary>
        /// wkhtmltoimage_set_progress_changed_callback.
        /// </summary>
        /// <param name="converter"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern int wkhtmltoimage_set_progress_changed_callback(
            IntPtr converter,
            [MarshalAs(UnmanagedType.FunctionPtr)] VoidCallback callback);

        /// <summary>
        /// wkhtmltoimage_set_finished_callback.
        /// </summary>
        /// <param name="converter"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern int wkhtmltoimage_set_finished_callback(
            IntPtr converter,
            [MarshalAs(UnmanagedType.FunctionPtr)] IntCallback callback);

        /// <summary>
        /// wkhtmltoimage_convert.
        /// </summary>
        /// <param name="converter"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern bool wkhtmltoimage_convert(IntPtr converter);

        /// <summary>
        /// wkhtmltoimage_current_phase.
        /// </summary>
        /// <param name="converter"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern int wkhtmltoimage_current_phase(IntPtr converter);

        /// <summary>
        /// wkhtmltoimage_phase_description.
        /// </summary>
        /// <param name="converter"></param>
        /// <param name="phase"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern IntPtr wkhtmltoimage_phase_description(IntPtr converter, int phase);

        /// <summary>
        /// wkhtmltoimage_progress_string.
        /// </summary>
        /// <param name="converter"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern IntPtr wkhtmltoimage_progress_string(IntPtr converter);

        /// <summary>
        /// wkhtmltoimage_phase_count.
        /// </summary>
        /// <param name="converter"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern int wkhtmltoimage_phase_count(IntPtr converter);

        /// <summary>
        /// wkhtmltoimage_http_error_code.
        /// </summary>
        /// <param name="converter"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern int wkhtmltoimage_http_error_code(IntPtr converter);

        /// <summary>
        /// wkhtmltoimage_get_output.
        /// </summary>
        /// <param name="converter"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset)]
        internal static extern int wkhtmltoimage_get_output(IntPtr converter, out IntPtr data);
    }
#pragma warning restore SA1300 // Element should begin with upper-case letter
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CA2101 // Specify marshaling for P/Invoke string arguments
#pragma warning restore CA1060 // Move pinvokes to native methods class
}
