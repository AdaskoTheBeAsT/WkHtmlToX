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

        private string GetLinuxVersion() => _runtimeIdentifier switch
        {
            WkHtmlToXRuntimeIdentifier.Centos => "centos",
            WkHtmlToXRuntimeIdentifier.Centos7 => "centos.7",
            WkHtmlToXRuntimeIdentifier.Centos8 => "centos.8",
            WkHtmlToXRuntimeIdentifier.Debian8X64 => "debian.8-x64",
            WkHtmlToXRuntimeIdentifier.Debian8X86 => "debian.8-x86",
            WkHtmlToXRuntimeIdentifier.Debian9X64 => "debian.9-x64",
            WkHtmlToXRuntimeIdentifier.Debian9X86 => "debian.9-x86",
            WkHtmlToXRuntimeIdentifier.Debian10X64 => "debian.10-x64",
            WkHtmlToXRuntimeIdentifier.Debian10X86 => "debian.10-x86",
            WkHtmlToXRuntimeIdentifier.Ubuntu1404X64 => "ubuntu.14.04-x64",
            WkHtmlToXRuntimeIdentifier.Ubuntu1404X86 => "ubuntu.14.04-x86",
            WkHtmlToXRuntimeIdentifier.Ubuntu1604X64 => "ubuntu.16.04-x64",
            WkHtmlToXRuntimeIdentifier.Ubuntu1604X86 => "ubuntu.16.04-x86",
            WkHtmlToXRuntimeIdentifier.Ubuntu1804X64 => "ubuntu.18.04-x64",
            WkHtmlToXRuntimeIdentifier.Ubuntu1804X86 => "ubuntu.18.04-x86",
            _ => throw new InvalidLinuxRuntimeIdentifierException(),
        };
    }
}
