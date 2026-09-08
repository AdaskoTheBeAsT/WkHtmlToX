using System;
using System.IO;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.Exceptions;

namespace AdaskoTheBeAsT.WkHtmlToX.Loaders;

internal sealed class LibraryLoaderFactory
    : ILibraryLoaderFactory
{
    public ILibraryLoader Create(
        WkHtmlToXConfiguration configuration)
    {
        if (configuration.NativeLibraryPath is not null && !IsAbsolutePath(configuration.NativeLibraryPath))
        {
            throw new ArgumentException("NativeLibraryPath must be absolute.", nameof(configuration));
        }

        var loader = CreateLoader(configuration);
        loader.ExplicitPath = configuration.NativeLibraryPath;
        return loader;
    }

    private static bool IsAbsolutePath(string path)
    {
#if NET8_0_OR_GREATER
        return Path.IsPathFullyQualified(path);
#else
        var root = Path.GetPathRoot(path);
        return Path.IsPathRooted(path) && root?.Length > 1 && root.EndsWith("\\", StringComparison.Ordinal);
#endif
    }

    private static LibraryLoaderBase CreateLoader(WkHtmlToXConfiguration configuration)
    {
        switch (configuration.PlatformId)
        {
            case (int)PlatformID.MacOSX:
                return new LibraryLoaderOsx();
            case (int)PlatformID.Unix:
            // Legacy mono value. See https://www.mono-project.com/docs/faq/technical/
            case 128:
                if (!configuration.RuntimeIdentifier.HasValue && configuration.NativeLibraryPath is null)
                {
                    throw new InvalidLinuxRuntimeIdentifierException();
                }

                return new LibraryLoaderLinux(configuration.RuntimeIdentifier ?? WkHtmlToXRuntimeIdentifier.Ubuntu2004X64);
            case (int)PlatformID.Win32NT:
            case (int)PlatformID.Win32S:
            case (int)PlatformID.Win32Windows:
            case (int)PlatformID.WinCE:
            case (int)PlatformID.Xbox:
                return new LibraryLoaderWindows();
            default:
                throw new InvalidPlatformIdentifierException();
        }
    }
}
