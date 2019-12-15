using System;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Exceptions;

namespace AdaskoTheBeAsT.WkHtmlToX.Loaders
{
    public class LibraryLoaderFactory
        : ILibraryLoaderFactory
    {
        public ILibraryLoader Create(WkHtmlToXRuntimeIdentifier? runtimeIdentifier)
        {
            switch ((int)Environment.OSVersion.Platform)
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
                default:
                    return new LibraryLoaderWindows();
            }
        }
    }
}
