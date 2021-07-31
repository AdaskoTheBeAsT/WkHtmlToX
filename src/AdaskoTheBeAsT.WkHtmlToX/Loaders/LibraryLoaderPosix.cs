using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using AdaskoTheBeAsT.WkHtmlToX.Exceptions;
using AdaskoTheBeAsT.WkHtmlToX.Native;

namespace AdaskoTheBeAsT.WkHtmlToX.Loaders
{
    [ExcludeFromCodeCoverage]
    internal abstract class LibraryLoaderPosix
        : LibraryLoaderBase
    {
        private IntPtr _libraryHandle;

        public override void Load()
        {
            var libraryName = GetLibraryName();
            var runtimeIdentifier = GetRuntimeIdentifier();

            var rootDirectory = GetCurrentDir();

            // Search a few different locations for our native assembly
            var paths = new[]
            {
                // This is where native libraries in our nupkg should end up
                GetRuntimeLibraryPath(rootDirectory, runtimeIdentifier, libraryName),

                // The build output folder
                GetCurrentDirectoryLibraryPath(rootDirectory, libraryName),
                Path.Combine("/usr/local/lib", libraryName),
                Path.Combine("/usr/lib", libraryName),
            };

            foreach (var path in paths)
            {
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                if (File.Exists(path))
                {
                    SystemPosixNativeMethods.dlerror();
                    var libPtr = SystemPosixNativeMethods.dlopen(path, SystemPosixNativeMethods.RTLD_NOW);
                    if (libPtr == IntPtr.Zero)
                    {
                        var errorPtr = SystemPosixNativeMethods.dlerror();
                        if (errorPtr != IntPtr.Zero)
                        {
                            var error = Marshal.PtrToStringAnsi(errorPtr);
                            throw new DllNotLoadedException($"dlopen failed: {path} : {error}");
                        }
                    }

                    _libraryHandle = libPtr;
                    return;
                }
            }

            throw new DllNotLoadedException();
        }

        public override void Release()
        {
            if (_libraryHandle == IntPtr.Zero)
            {
                return;
            }

            var retVal = SystemPosixNativeMethods.dlclose(_libraryHandle);
            if (retVal != 0)
            {
                var errorPtr = SystemPosixNativeMethods.dlerror();
                if (errorPtr != IntPtr.Zero)
                {
                    var error = Marshal.PtrToStringAnsi(errorPtr);
                    throw new DllUnloadFailedException($"dlclose failed: {error}");
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Release();
            }
        }

        protected abstract string GetLibraryName();

        protected abstract string GetRuntimeIdentifier();
    }
}
