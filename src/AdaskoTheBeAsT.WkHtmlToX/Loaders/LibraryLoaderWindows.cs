using System;
using System.IO;
#if NET9_0_OR_GREATER
using System.Threading;
#endif
using AdaskoTheBeAsT.WkHtmlToX.Exceptions;
using AdaskoTheBeAsT.WkHtmlToX.Native;

namespace AdaskoTheBeAsT.WkHtmlToX.Loaders;

internal sealed class LibraryLoaderWindows
    : LibraryLoaderBase
{
    private const string LibraryName = "wkhtmltox.dll";

#if NET9_0_OR_GREATER
    private static readonly Lock SyncLock = new();
#else
    private static readonly object SyncLock = new();
#endif

    private SafeLibraryHandle? _libraryHandle;

    public override void Load()
    {
        lock (SyncLock)
        {
            // Already loaded
            if (_libraryHandle != null && !_libraryHandle.IsClosed)
            {
                return;
            }

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
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                if (File.Exists(path))
                {
                    var libHandle = SystemWindowsNativeMethods.LoadLibraryEx(
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
    }

    public override void Release()
    {
        SafeLibraryHandle? handleToDispose = null;
        lock (SyncLock)
        {
            if (_libraryHandle == null)
            {
                return;
            }

            handleToDispose = _libraryHandle;
            _libraryHandle = null;
        }

        if (handleToDispose != null && !handleToDispose.IsClosed)
        {
            handleToDispose.Dispose();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            SafeLibraryHandle? handleToDispose = null;
            lock (SyncLock)
            {
                if (_libraryHandle == null)
                {
                    return;
                }

                handleToDispose = _libraryHandle;
                _libraryHandle = null;
            }

            if (handleToDispose != null && !handleToDispose.IsClosed)
            {
                handleToDispose.Dispose();
            }
        }
    }
}
