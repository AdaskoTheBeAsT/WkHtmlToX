using System;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;

namespace AdaskoTheBeAsT.WkHtmlToX.Loaders
{
    public class LibraryLoaderFactory
        : ILibraryLoaderFactory
    {
        public ILibraryLoader Create()
        {
            switch ((int)Environment.OSVersion.Platform)
            {
                case (int)PlatformID.MacOSX:
                    return new LibraryLoaderOsx();
                case (int)PlatformID.Unix:
                // Legacy mono value. See https://www.mono-project.com/docs/faq/technical/
                case 128:
                    return new LibraryLoaderLinux();
                default:
                    return new LibraryLoaderWindows();
            }
        }
    }
}
