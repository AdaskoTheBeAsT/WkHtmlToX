using System;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Exceptions;
using AdaskoTheBeAsT.WkHtmlToX.Utils;

namespace AdaskoTheBeAsT.WkHtmlToX.Modules
{
    internal class WkHtmlToXModuleFactory
        : IWkHtmlToXModuleFactory
    {
        public IWkHtmlToXModule GetModule(int platformId, ModuleKind moduleKind)
        {
            return GetModule(platformId, moduleKind, new BufferManager());
        }

        internal IWkHtmlToXModule GetModule(int platformId, ModuleKind moduleKind, IBufferManager bufferManager)
        {
            switch (platformId)
            {
                case (int)PlatformID.MacOSX:
                case (int)PlatformID.Unix:
                // Legacy mono value. See https://www.mono-project.com/docs/faq/technical/
                case 128:
                    if (moduleKind == ModuleKind.Pdf)
                    {
                        return new WkHtmlToPdfPosixCommonModule(bufferManager);
                    }

                    return new WkHtmlToImagePosixCommonModule(bufferManager);
                case (int)PlatformID.Win32NT:
                case (int)PlatformID.Win32S:
                case (int)PlatformID.Win32Windows:
                case (int)PlatformID.WinCE:
                case (int)PlatformID.Xbox:
                    if (moduleKind == ModuleKind.Pdf)
                    {
                        return new WkHtmlToPdfWindowsCommonModule(bufferManager);
                    }

                    return new WkHtmlToImageWindowsCommonModule(bufferManager);
                default:
                    throw new InvalidPlatformIdentifierException();
            }
        }
    }
}
