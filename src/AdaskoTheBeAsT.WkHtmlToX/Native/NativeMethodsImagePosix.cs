#nullable enable
using System;
using System.Runtime.InteropServices;
using System.Security;
using AdaskoTheBeAsT.WkHtmlToX.Utils;

namespace AdaskoTheBeAsT.WkHtmlToX.Native
{
#pragma warning disable CA1060 // Move pinvokes to native methods class
#pragma warning disable CA2101 // Specify marshaling for P/Invoke string arguments
    internal static class NativeMethodsImagePosix
    {
        private const CallingConvention CallConvention = CallingConvention.Cdecl;

        /// <summary>
        /// wkhtmltoimage_init.
        /// </summary>
        /// <param name="useGraphics"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, EntryPoint = "wkhtmltoimage_init", CharSet = NativeLib.Charset, CallingConvention = CallConvention)]

        internal static extern int Initialize(int useGraphics);

        /// <summary>
        /// wkhtmltoimage_deinit.
        /// </summary>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, EntryPoint = "wkhtmltoimage_deinit", CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern int Terminate();

        /// <summary>
        /// wkhtmltoimage_extended_qt.
        /// </summary>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, EntryPoint= "wkhtmltoimage_extended_qt", CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern int ExtendedQt();

        /// <summary>
        /// wkhtmltoimage_version.
        /// </summary>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, EntryPoint = "wkhtmltoimage_version", CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern IntPtr GetVersion();

        /// <summary>
        /// wkhtmltoimage_create_global_settings.
        /// </summary>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, EntryPoint = "wkhtmltoimage_create_global_settings", CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern IntPtr CreateGlobalSettings();

        /// <summary>
        /// wkhtmltoimage_destroy_global_settings.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, EntryPoint = "wkhtmltoimage_destroy_global_settings", CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern int DestroyGlobalSettings(IntPtr settings);

        /// <summary>
        /// wkhtmltoimage_set_global_setting.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, EntryPoint = "wkhtmltoimage_set_global_setting", CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern int SetGlobalSettings(
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
        [DllImport(NativeLib.DllName, EntryPoint = "wkhtmltoimage_get_global_setting", CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern int GetGlobalSettings(
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
        [DllImport(NativeLib.DllName, EntryPoint = "wkhtmltoimage_create_converter", CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern IntPtr CreateConverter(IntPtr globalSettings);

        /// <summary>
        /// wkhtmltoimage_destroy_converter.
        /// </summary>
        /// <param name="converter"></param>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, EntryPoint = "wkhtmltoimage_destroy_converter", CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern void DestroyConverter(IntPtr converter);

        /// <summary>
        /// wkhtmltoimage_set_warning_callback.
        /// </summary>
        /// <param name="converter"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, EntryPoint = "wkhtmltoimage_set_warning_callback", CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern int SetWarningCallback(
            IntPtr converter,
            [MarshalAs(UnmanagedType.FunctionPtr)] StringCallback callback);

        /// <summary>
        /// wkhtmltoimage_set_error_callback.
        /// </summary>
        /// <param name="converter"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, EntryPoint = "wkhtmltoimage_set_error_callback", CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern int SetErrorCallback(
            IntPtr converter,
            [MarshalAs(UnmanagedType.FunctionPtr)] StringCallback callback);

        /// <summary>
        /// wkhtmltoimage_set_phase_changed_callback.
        /// </summary>
        /// <param name="converter"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, EntryPoint = "wkhtmltoimage_set_phase_changed_callback", CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern int SetPhaseChangedCallback(
            IntPtr converter,
            [MarshalAs(UnmanagedType.FunctionPtr)] VoidCallback callback);

        /// <summary>
        /// wkhtmltoimage_set_progress_changed_callback.
        /// </summary>
        /// <param name="converter"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, EntryPoint = "wkhtmltoimage_set_progress_changed_callback", CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern int SetProgressChangedCallback(
            IntPtr converter,
            [MarshalAs(UnmanagedType.FunctionPtr)] VoidCallback callback);

        /// <summary>
        /// wkhtmltoimage_set_finished_callback.
        /// </summary>
        /// <param name="converter"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, EntryPoint = "wkhtmltoimage_set_finished_callback", CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern int SetFinishedCallback(
            IntPtr converter,
            [MarshalAs(UnmanagedType.FunctionPtr)] IntCallback callback);

        /// <summary>
        /// wkhtmltoimage_convert.
        /// </summary>
        /// <param name="converter"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, EntryPoint = "wkhtmltoimage_convert", CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern bool Convert(IntPtr converter);

        /// <summary>
        /// wkhtmltoimage_current_phase.
        /// </summary>
        /// <param name="converter"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, EntryPoint = "wkhtmltoimage_current_phase", CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern int GetCurrentPhase(IntPtr converter);

        /// <summary>
        /// wkhtmltoimage_phase_description.
        /// </summary>
        /// <param name="converter"></param>
        /// <param name="phase"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, EntryPoint = "wkhtmltoimage_phase_description", CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern IntPtr GetPhaseDescription(IntPtr converter, int phase);

        /// <summary>
        /// wkhtmltoimage_progress_string.
        /// </summary>
        /// <param name="converter"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, EntryPoint = "wkhtmltoimage_progress_string", CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern IntPtr GetProgressDescription(IntPtr converter);

        /// <summary>
        /// wkhtmltoimage_phase_count.
        /// </summary>
        /// <param name="converter"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, EntryPoint = "wkhtmltoimage_phase_count", CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern int GetPhaseCount(IntPtr converter);

        /// <summary>
        /// wkhtmltoimage_http_error_code.
        /// </summary>
        /// <param name="converter"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, EntryPoint = "wkhtmltoimage_http_error_code", CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern int GetHttpErrorCode(IntPtr converter);

        /// <summary>
        /// wkhtmltoimage_get_output.
        /// </summary>
        /// <param name="converter"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLib.DllName, EntryPoint = "wkhtmltoimage_get_output", CharSet = NativeLib.Charset, CallingConvention = CallConvention)]
        internal static extern int GetOutput(IntPtr converter, out IntPtr data);
    }
#pragma warning restore CA2101 // Specify marshaling for P/Invoke string arguments
#pragma warning restore CA1060 // Move pinvokes to native methods class
}
