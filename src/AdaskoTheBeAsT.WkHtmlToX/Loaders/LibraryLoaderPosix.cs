using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using AdaskoTheBeAsT.Interop.Unmanaged;
using AdaskoTheBeAsT.WkHtmlToX.Exceptions;

namespace AdaskoTheBeAsT.WkHtmlToX.Loaders;

[ExcludeFromCodeCoverage]
#pragma warning disable CA2213 // Field is disposed via Release()/Dispose(bool)
internal abstract class LibraryLoaderPosix
    : LibraryLoaderBase
{
    private UnmanagedLibrary? _library;

    public override void Load()
    {
        if (_library is not null)
        {
            return;
        }

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

            if (!File.Exists(path))
            {
                continue;
            }

            try
            {
#pragma warning disable IDISP003 // Dispose previous before re-assigning.
                _library = new UnmanagedLibrary(path);
#pragma warning restore IDISP003
                return;
            }
            catch (Win32Exception ex)
            {
                throw new DllNotLoadedException($"dlopen failed: {path} : {ex.Message}", ex);
            }
        }

        throw new DllNotLoadedException();
    }

    public override void Release()
    {
        var libraryToDispose = _library;
#pragma warning disable IDISP003 // Dispose previous before re-assigning.
        _library = null;
#pragma warning restore IDISP003
        libraryToDispose?.Dispose();
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
#pragma warning restore CA2213
