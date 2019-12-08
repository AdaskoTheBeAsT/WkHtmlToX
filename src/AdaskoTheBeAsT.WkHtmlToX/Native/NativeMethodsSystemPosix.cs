using System;
using System.Runtime.InteropServices;

namespace AdaskoTheBeAsT.WkHtmlToX.Native
{
#pragma warning disable CA1060 // Move pinvokes to native methods class
#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1310 // Field names should not contain underscore
    internal static class NativeMethodsSystemPosix
    {
        // ReSharper disable once InconsistentNaming
        internal const int RTLD_NOW = 2;

        [DllImport("libdl", CharSet = CharSet.Unicode)]
        internal static extern IntPtr dlopen(string fileName, int flags);

        [DllImport("libdl", CharSet = CharSet.Unicode)]
        internal static extern IntPtr dlsym(IntPtr handle, string symbol);

        [DllImport("libdl")]
        internal static extern int dlclose(IntPtr handle);

        [DllImport("libdl")]
        internal static extern IntPtr dlerror();
    }
#pragma warning restore SA1310 // Field names should not contain underscore
#pragma warning restore SA1300 // Element should begin with upper-case letter
#pragma warning restore CA1060 // Move pinvokes to native methods class
}
