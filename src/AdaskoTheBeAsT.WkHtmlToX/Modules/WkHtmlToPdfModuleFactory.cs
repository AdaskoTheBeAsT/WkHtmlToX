using System;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Exceptions;

namespace AdaskoTheBeAsT.WkHtmlToX.Modules
{
    internal class WkHtmlToPdfModuleFactory
        : IWkHtmlToPdfModuleFactory
    {
        public IWkHtmlToPdfModule GetModule(int platformId)
        {
            switch (platformId)
            {
                case (int)PlatformID.MacOSX:
                case (int)PlatformID.Unix:
                // Legacy mono value. See https://www.mono-project.com/docs/faq/technical/
                case 128:
                    return new WkHtmlToPdfPosixAdditionalModule();
                case (int)PlatformID.Win32NT:
                case (int)PlatformID.Win32S:
                case (int)PlatformID.Win32Windows:
                case (int)PlatformID.WinCE:
                case (int)PlatformID.Xbox:
                    return new WkHtmlToPdfWindowsAdditionalModule();
                default:
                    throw new InvalidPlatformIdentifierException();
            }
        }
    }
}
