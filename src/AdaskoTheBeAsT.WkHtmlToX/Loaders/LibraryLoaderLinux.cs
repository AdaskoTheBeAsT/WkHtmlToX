using AdaskoTheBeAsT.WkHtmlToX.Exceptions;

namespace AdaskoTheBeAsT.WkHtmlToX.Loaders
{
    public class LibraryLoaderLinux
        : LibraryLoaderPosix
    {
        private readonly WkHtmlToXRuntimeIdentifier _runtimeIdentifier;

        public LibraryLoaderLinux(
            WkHtmlToXRuntimeIdentifier runtimeIdentifier)
        {
            _runtimeIdentifier = runtimeIdentifier;
        }

        protected override string GetLibraryName() => "libwkhtmltox.so";

        protected override string GetRuntimeIdentifier() => $"{GetLinuxVersion()}";

        private string GetLinuxVersion()
        {
            switch (_runtimeIdentifier)
            {
                case WkHtmlToXRuntimeIdentifier.Centos:
                    return "centos";
                case WkHtmlToXRuntimeIdentifier.Centos7:
                    return "centos.7";
                case WkHtmlToXRuntimeIdentifier.Centos8:
                    return "centos.8";
                case WkHtmlToXRuntimeIdentifier.Debian8X64:
                    return "debian.8-x64";
                case WkHtmlToXRuntimeIdentifier.Debian8X86:
                    return "debian.8-x86";
                case WkHtmlToXRuntimeIdentifier.Debian9X64:
                    return "debian.9-x64";
                case WkHtmlToXRuntimeIdentifier.Debian9X86:
                    return "debian.9-x86";
                case WkHtmlToXRuntimeIdentifier.Debian10X64:
                    return "debian.10-x64";
                case WkHtmlToXRuntimeIdentifier.Debian10X86:
                    return "debian.10-x86";
                case WkHtmlToXRuntimeIdentifier.Ubuntu1404X64:
                    return "ubuntu.14.04-x64";
                case WkHtmlToXRuntimeIdentifier.Ubuntu1404X86:
                    return "ubuntu.14.04-x86";
                case WkHtmlToXRuntimeIdentifier.Ubuntu1604X64:
                    return "ubuntu.16.04-x64";
                case WkHtmlToXRuntimeIdentifier.Ubuntu1604X86:
                    return "ubuntu.16.04-x86";
                case WkHtmlToXRuntimeIdentifier.Ubuntu1804X64:
                    return "ubuntu.18.04-x64";
                case WkHtmlToXRuntimeIdentifier.Ubuntu1804X86:
                    return "ubuntu.18.04-x86";
                default:
                    throw new InvalidLinuxRuntimeIdentifierException();
            }
        }
    }
}
