using System;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;

namespace AdaskoTheBeAsT.WkHtmlToX.Modules
{
    internal class WkHtmlToPdfModuleFactory
        : IWkHtmlToPdfModuleFactory
    {
        public IWkHtmlToPdfModule GetModule()
        {
            switch ((int)Environment.OSVersion.Platform)
            {
                case (int)PlatformID.MacOSX:
                case (int)PlatformID.Unix:
                // Legacy mono value. See https://www.mono-project.com/docs/faq/technical/
                case 128:
                    return new WkHtmlToPdfPosixAdditionalModule();
                default:
                    return new WkHtmlToPdfWindowsAdditionalModule();
            }
        }
    }
}
