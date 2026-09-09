using System;
using System.IO;
using System.Runtime.InteropServices;
#if NET9_0_OR_GREATER
using System.Threading;
#endif

using AdaskoTheBeAsT.Interop.Unmanaged;
using AdaskoTheBeAsT.WkHtmlToX.Native;

namespace AdaskoTheBeAsT.WkHtmlToX.Loaders;

internal static class NativeLibraryBinding
{
#if NET9_0_OR_GREATER
    private static readonly Lock SyncLock = new();
#else
    private static readonly object SyncLock = new();
#endif

    // P/Invoke caches function addresses. Retain one module for the process lifetime.
    private static SafeLibraryHandle? _handle;
    private static string? _path;

#if NET8_0_OR_GREATER
    static NativeLibraryBinding()
    {
        NativeLibrary.SetDllImportResolver(
            typeof(NativeLib).Assembly,
#pragma warning disable S3869 // Static ownership pins this handle for the entire process; it is never closed.
            (name, _, _) => string.Equals(name, NativeLib.DllName, StringComparison.Ordinal)
                ? _handle?.DangerousGetHandle() ?? throw new InvalidOperationException("Load the native library before invoking it.")
                : IntPtr.Zero);
#pragma warning restore S3869
    }
#endif

    private static StringComparison PathComparison =>
        Environment.OSVersion.Platform == PlatformID.Win32NT ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    internal static void Load(string path)
    {
        path = Path.GetFullPath(path);
        lock (SyncLock)
        {
            if (_handle is not null)
            {
                if (!string.Equals(_path, path, PathComparison))
                {
                    throw new InvalidOperationException("A different native library is already bound. Restart the process to change it.");
                }

                return;
            }

#if !NET8_0_OR_GREATER
            if (Environment.OSVersion.Platform != PlatformID.Win32NT
                || !string.Equals(Path.GetFileName(path), "wkhtmltox.dll", StringComparison.OrdinalIgnoreCase))
            {
                throw new PlatformNotSupportedException(".NET Framework requires Windows and a native library named wkhtmltox.dll.");
            }

            if (NativeMethods.GetModuleHandle("wkhtmltox.dll") != IntPtr.Zero)
            {
                throw new InvalidOperationException("wkhtmltox was loaded outside this engine. Its binding cannot be verified.");
            }
#endif

#pragma warning disable IDISP003 // Assigned once under the lock and deliberately retained until process exit.
            _handle = UnmanagedLibrary.LoadLibrary(
                path,
                LoadLibraryFlags.LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR | LoadLibraryFlags.LOAD_LIBRARY_SEARCH_SYSTEM32);
#pragma warning restore IDISP003
            _path = path;
        }
    }

#if !NET8_0_OR_GREATER
    private static class NativeMethods
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetModuleHandleW")]
        internal static extern IntPtr GetModuleHandle(string moduleName);
    }
#endif
}
