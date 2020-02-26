using System;
using System.IO;
using AdaskoTheBeAsT.WkHtmlToX.Exceptions;
using AdaskoTheBeAsT.WkHtmlToX.Native;

namespace AdaskoTheBeAsT.WkHtmlToX.Loaders
{
    public class LibraryLoaderWindows
        : LibraryLoaderBase
    {
        private const string LibraryName = "wkhtmltox.dll";

        private SafeLibraryHandle? _libraryHandle;

        public override void Load()
        {
            // https://docs.microsoft.com/en-us/dotnet/core/rid-catalog
            var runtimeIdentifier = $"win-{GetProcessorArchitecture()}";

            var rootDirectory = GetCurrentDir();

            // Search a few different locations for our native assembly
            var paths = new[]
            {
                // This is where native libraries in our nupkg should end up
                GetRuntimeLibraryPath(rootDirectory, runtimeIdentifier, LibraryName),

                // The build output folder
                GetCurrentDirectoryLibraryPath(rootDirectory, LibraryName),
            };

            foreach (var path in paths)
            {
                if (path == null)
                {
                    continue;
                }

                if (File.Exists(path))
                {
                    var libHandle = NativeMethodsSystemWindows.LoadLibraryEx(
                        path,
                        IntPtr.Zero,
                        LoadLibraryFlags.LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR | LoadLibraryFlags.LOAD_LIBRARY_SEARCH_SYSTEM32);
                    if (libHandle.IsInvalid)
                    {
                        throw new DllNotLoadedException($"LoadLibrary failed: {path}");
                    }

                    _libraryHandle = libHandle;
                    return;
                }
            }

            throw new DllNotLoadedException();
        }

        public override void Release()
        {
            if (_libraryHandle == null)
            {
                return;
            }

            if (!_libraryHandle.IsClosed)
            {
                _libraryHandle.Dispose();
            }

            _libraryHandle = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_libraryHandle == null)
                {
                    return;
                }

                if (!_libraryHandle.IsClosed)
                {
                    _libraryHandle.Dispose();
                }

                _libraryHandle = null;
            }
        }
    }
}
