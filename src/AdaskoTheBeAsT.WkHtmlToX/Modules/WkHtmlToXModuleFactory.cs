using System;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;

namespace AdaskoTheBeAsT.WkHtmlToX.Modules
{
    internal class WkHtmlToXModuleFactory
        : IWkHtmlToXModuleFactory
    {
        public IWkHtmlToXModule GetModule(ModuleKind moduleKind)
        {
            switch ((int)Environment.OSVersion.Platform)
            {
                case (int)PlatformID.MacOSX:
                case (int)PlatformID.Unix:
                // Legacy mono value. See https://www.mono-project.com/docs/faq/technical/
                case 128:
                    if (moduleKind == ModuleKind.Pdf)
                    {
                        return new WkHtmlToPdfPosixCommonModule();
                    }

                    return new WkHtmlToImagePosixCommonModule();
                default:
                    if (moduleKind == ModuleKind.Pdf)
                    {
                        return new WkHtmlToPdfWindowsCommonModule();
                    }

                    return new WkHtmlToImageWindowsCommonModule();
            }
        }
    }
}
