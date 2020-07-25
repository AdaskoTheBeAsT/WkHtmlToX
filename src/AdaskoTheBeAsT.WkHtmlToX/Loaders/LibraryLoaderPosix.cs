using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using AdaskoTheBeAsT.WkHtmlToX.Exceptions;
using AdaskoTheBeAsT.WkHtmlToX.Native;

namespace AdaskoTheBeAsT.WkHtmlToX.Loaders
{
    [ExcludeFromCodeCoverage]
    public abstract class LibraryLoaderPosix
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
                if (path == null)
                {
                    continue;
                }

                if (File.Exists(path))
                {
                    NativeMethodsSystemPosix.dlerror();
                    var libPtr = NativeMethodsSystemPosix.dlopen(path, NativeMethodsSystemPosix.RTLD_NOW);
                    if (libPtr == IntPtr.Zero)
                    {
                        var errorPtr = NativeMethodsSystemPosix.dlerror();
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

            var retVal = NativeMethodsSystemPosix.dlclose(_libraryHandle);
            if (retVal != 0)
            {
                var errorPtr = NativeMethodsSystemPosix.dlerror();
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

            base.Dispose(disposing);
        }

        protected abstract string GetLibraryName();

        protected abstract string GetRuntimeIdentifier();
    }
}
