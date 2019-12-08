using System;
using System.IO;
using System.Runtime.InteropServices;
using AdaskoTheBeAsT.WkHtmlToX.Exceptions;
using AdaskoTheBeAsT.WkHtmlToX.Native;

namespace AdaskoTheBeAsT.WkHtmlToX.Loaders
{
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
                        var error = Marshal.PtrToStringAnsi(NativeMethodsSystemPosix.dlerror());
                        throw new DllNotLoadedException($"dlopen failed: {path} : {error}");
                    }

                    _libraryHandle = libPtr;
                    return;
                }
            }

            throw new DllNotLoadedException();
        }

        public override void Release()
        {
            _ = NativeMethodsSystemPosix.dlclose(_libraryHandle);
        }

        protected override void Dispose(
            bool disposing)
        {
            if (disposing)
            {
                _ = NativeMethodsSystemPosix.dlclose(_libraryHandle);
            }
        }

        protected abstract string GetLibraryName();

        protected abstract string GetRuntimeIdentifier();
    }
}
