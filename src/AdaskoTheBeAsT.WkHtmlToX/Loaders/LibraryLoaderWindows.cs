using System.ComponentModel;
using System.IO;
using AdaskoTheBeAsT.WkHtmlToX.Exceptions;

namespace AdaskoTheBeAsT.WkHtmlToX.Loaders;

internal sealed class LibraryLoaderWindows
    : LibraryLoaderBase
{
    private const string LibraryName = "wkhtmltox.dll";

    public override void Load()
    {
        // https://docs.microsoft.com/en-us/dotnet/core/rid-catalog
        var runtimeIdentifier = $"win-{GetProcessorArchitecture()}";
        var paths = GetPaths(runtimeIdentifier, LibraryName);

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
                throw new DllNotLoadedException($"LoadLibrary failed: {path}", ex);
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
}
