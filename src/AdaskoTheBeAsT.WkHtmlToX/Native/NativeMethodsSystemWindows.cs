using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using AdaskoTheBeAsT.WkHtmlToX.Loaders;

namespace AdaskoTheBeAsT.WkHtmlToX.Native
{
#pragma warning disable CA1060 // Move pinvokes to native methods class
    internal static class NativeMethodsSystemWindows
    {
        private const string KernelLib = "kernel32";

        [SuppressUnmanagedCodeSecurity]
        [DllImport(KernelLib, CharSet = CharSet.Auto, BestFitMapping = false, SetLastError = true)]
        internal static extern SafeLibraryHandle LoadLibrary(string fileName);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(KernelLib, CharSet = CharSet.Auto, BestFitMapping = false, SetLastError = true)]
        internal static extern SafeLibraryHandle LoadLibraryEx(
            string fileName,
            IntPtr hFile,
            [MarshalAs(UnmanagedType.U4)] LoadLibraryFlags dwFlags);

        [SuppressUnmanagedCodeSecurity]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport(KernelLib, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FreeLibrary(IntPtr hModule);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(KernelLib, CharSet = CharSet.Ansi, EntryPoint = "GetProcAddress", ExactSpelling = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        internal static extern IntPtr GetProcAddress(SafeLibraryHandle hModule, string procname);
    }
#pragma warning restore CA1060 // Move pinvokes to native methods class
}
