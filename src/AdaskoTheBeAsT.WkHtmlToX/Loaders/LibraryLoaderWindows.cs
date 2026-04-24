using System.ComponentModel;
using System.IO;
#if NET9_0_OR_GREATER
using System.Threading;
#endif
using AdaskoTheBeAsT.Interop.Unmanaged;
using AdaskoTheBeAsT.WkHtmlToX.Exceptions;

namespace AdaskoTheBeAsT.WkHtmlToX.Loaders;

#pragma warning disable CA2213 // Field is disposed via Release()/Dispose(bool)
internal sealed class LibraryLoaderWindows
    : LibraryLoaderBase
{
    private const string LibraryName = "wkhtmltox.dll";

#if NET9_0_OR_GREATER
    private static readonly Lock SyncLock = new();
#else
    private static readonly object SyncLock = new();
#endif

    private UnmanagedLibrary? _library;

    public override void Load()
    {
        lock (SyncLock)
        {
            if (_library is not null)
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

                if (!File.Exists(path))
                {
                    continue;
                }

                try
                {
#pragma warning disable IDISP003 // Dispose previous before re-assigning.
                    _library = new UnmanagedLibrary(
                        path,
                        LoadLibraryFlags.LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR | LoadLibraryFlags.LOAD_LIBRARY_SEARCH_SYSTEM32);
#pragma warning restore IDISP003
                    return;
                }
                catch (Win32Exception ex)
                {
                    throw new DllNotLoadedException($"LoadLibrary failed: {path}", ex);
                }
            }

            throw new DllNotLoadedException();
        }
    }

    public override void Release()
    {
        UnmanagedLibrary? libraryToDispose;
        lock (SyncLock)
        {
            libraryToDispose = _library;
#pragma warning disable IDISP003 // Dispose previous before re-assigning.
            _library = null;
#pragma warning restore IDISP003
        }

        libraryToDispose?.Dispose();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Release();
        }
    }
}
#pragma warning restore CA2213
