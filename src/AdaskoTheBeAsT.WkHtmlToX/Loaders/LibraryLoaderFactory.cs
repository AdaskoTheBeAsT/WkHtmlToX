using System;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Exceptions;

namespace AdaskoTheBeAsT.WkHtmlToX.Loaders
{
    public class LibraryLoaderFactory
        : ILibraryLoaderFactory
    {
        public ILibraryLoader Create(int platformId, WkHtmlToXRuntimeIdentifier? runtimeIdentifier)
        {
            switch (platformId)
            {
                case (int)PlatformID.MacOSX:
                    return new LibraryLoaderOsx();
                case (int)PlatformID.Unix:
                // Legacy mono value. See https://www.mono-project.com/docs/faq/technical/
                case 128:
                    if (!runtimeIdentifier.HasValue)
                    {
                        throw new InvalidLinuxRuntimeIdentifierException();
                    }

                    return new LibraryLoaderLinux(runtimeIdentifier.Value);
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
}
