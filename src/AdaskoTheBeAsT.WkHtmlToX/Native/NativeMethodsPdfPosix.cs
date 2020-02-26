using System;
using System.Runtime.InteropServices;
using System.Security;
using AdaskoTheBeAsT.WkHtmlToX.Utils;

namespace AdaskoTheBeAsT.WkHtmlToX.Native
{
#pragma warning disable CA1060 // Move pinvokes to native methods class
#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable CA2101 // Specify marshaling for P/Invoke string arguments
    internal static class NativeMethodsPdfPosix
    {
        private const CallingConvention CallConvention = CallingConvention.Cdecl;

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern int wkhtmltopdf_init(int useGraphics);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern int wkhtmltopdf_deinit();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern int wkhtmltopdf_extended_qt();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern IntPtr wkhtmltopdf_version();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern IntPtr wkhtmltopdf_create_global_settings();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern int wkhtmltopdf_destroy_global_settings(IntPtr settings);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern IntPtr wkhtmltopdf_create_object_settings();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern int wkhtmltopdf_destroy_object_settings(IntPtr settings);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern int wkhtmltopdf_set_global_setting(
            IntPtr settings,
            [MarshalAs((short)CustomUnmanagedType.LPUTF8Str)] string name,
            [MarshalAs((short)CustomUnmanagedType.LPUTF8Str)] string? value);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern int wkhtmltopdf_get_global_setting(
            IntPtr settings,
            [MarshalAs((short)CustomUnmanagedType.LPUTF8Str)] string name,
            [MarshalAs(UnmanagedType.LPArray)] byte[] value,
            int valueSize);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern int wkhtmltopdf_set_object_setting(
            IntPtr settings,
            [MarshalAs((short)CustomUnmanagedType.LPUTF8Str)] string name,
            [MarshalAs((short)CustomUnmanagedType.LPUTF8Str)] string? value);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern int wkhtmltopdf_get_object_setting(
            IntPtr settings,
            [MarshalAs((short)CustomUnmanagedType.LPUTF8Str)] string name,
            [MarshalAs(UnmanagedType.LPArray)] byte[] value,
            int valueSize);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern IntPtr wkhtmltopdf_create_converter(IntPtr globalSettings);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern void wkhtmltopdf_destroy_converter(IntPtr converter);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern int wkhtmltopdf_set_warning_callback(
            IntPtr converter,
            [MarshalAs(UnmanagedType.FunctionPtr)] StringCallback callback);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern int wkhtmltopdf_set_error_callback(
            IntPtr converter,
            [MarshalAs(UnmanagedType.FunctionPtr)] StringCallback callback);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern int wkhtmltopdf_set_phase_changed_callback(
            IntPtr converter,
            [MarshalAs(UnmanagedType.FunctionPtr)] VoidCallback callback);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern int wkhtmltopdf_set_progress_changed_callback(
            IntPtr converter,
            [MarshalAs(UnmanagedType.FunctionPtr)] VoidCallback callback);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern int wkhtmltopdf_set_finished_callback(
            IntPtr converter,
            [MarshalAs(UnmanagedType.FunctionPtr)] IntCallback callback);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern bool wkhtmltopdf_convert(IntPtr converter);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern void wkhtmltopdf_add_object(
            IntPtr converter,
            IntPtr objectSettings,
            byte[] data);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern void wkhtmltopdf_add_object(
            IntPtr converter,
            IntPtr objectSettings,
            [MarshalAs((short)CustomUnmanagedType.LPUTF8Str)] string data);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern int wkhtmltopdf_current_phase(IntPtr converter);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern IntPtr wkhtmltopdf_phase_description(IntPtr converter, int phase);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern IntPtr wkhtmltopdf_progress_string(IntPtr converter);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern int wkhtmltopdf_phase_count(IntPtr converter);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern int wkhtmltopdf_http_error_code(IntPtr converter);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern int wkhtmltopdf_get_output(IntPtr converter, out IntPtr data);
    }
#pragma warning restore CA2101 // Specify marshaling for P/Invoke string arguments
#pragma warning restore SA1300 // Element should begin with upper-case letter
#pragma warning restore CA1060 // Move pinvokes to native methods class
}
