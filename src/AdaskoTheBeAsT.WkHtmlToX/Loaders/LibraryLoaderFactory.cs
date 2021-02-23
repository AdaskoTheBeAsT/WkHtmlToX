using System;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.Exceptions;

namespace AdaskoTheBeAsT.WkHtmlToX.Loaders
{
    internal class LibraryLoaderFactory
        : ILibraryLoaderFactory
    {
        public ILibraryLoader Create(
            WkHtmlToXConfiguration wkHtmlToXConfiguration)
        {
            switch (wkHtmlToXConfiguration.PlatformId)
            {
                case (int)PlatformID.MacOSX:
                    return new LibraryLoaderOsx();
                case (int)PlatformID.Unix:
                // Legacy mono value. See https://www.mono-project.com/docs/faq/technical/
                case 128:
                    if (!wkHtmlToXConfiguration.RuntimeIdentifier.HasValue)
                    {
                        throw new InvalidLinuxRuntimeIdentifierException();
                    }

                    return new LibraryLoaderLinux(wkHtmlToXConfiguration.RuntimeIdentifier.Value);
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
