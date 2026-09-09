using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using AdaskoTheBeAsT.WkHtmlToX.Exceptions;

namespace AdaskoTheBeAsT.WkHtmlToX.Loaders;

[ExcludeFromCodeCoverage]
internal abstract class LibraryLoaderPosix
    : LibraryLoaderBase
{
    public override void Load()
    {
        _ = GetProcessorArchitecture();
        var libraryName = GetLibraryName();
        var runtimeIdentifier = ExplicitPath is null ? GetRuntimeIdentifier() : string.Empty;
        var paths = GetPaths(runtimeIdentifier, libraryName);

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
                NativeLibraryBinding.Load(path);
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
        // Native code remains bound for the process lifetime; sessions still terminate.
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
