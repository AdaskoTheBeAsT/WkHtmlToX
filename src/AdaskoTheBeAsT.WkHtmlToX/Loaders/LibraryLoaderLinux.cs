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

        protected override string GetLibraryName() => "libwkhtmltox.so.0.12.6";

        protected override string GetRuntimeIdentifier() => $"{GetLinuxVersion()}";

        private string GetLinuxVersion() => _runtimeIdentifier switch
        {
            WkHtmlToXRuntimeIdentifier.AmazonLinux2 => "amazonlinux.2",
            WkHtmlToXRuntimeIdentifier.Centos6 => "centos.6",
            WkHtmlToXRuntimeIdentifier.Centos7 => "centos.7",
            WkHtmlToXRuntimeIdentifier.Centos8 => "centos.8",
            WkHtmlToXRuntimeIdentifier.Debian9X64 => "debian.9-x64",
            WkHtmlToXRuntimeIdentifier.Debian9X86 => "debian.9-x86",
            WkHtmlToXRuntimeIdentifier.Debian10X64 => "debian.10-x64",
            WkHtmlToXRuntimeIdentifier.Debian10X86 => "debian.10-x86",
            WkHtmlToXRuntimeIdentifier.OpenSuseLeap15 => "opensuseleap.15",
            WkHtmlToXRuntimeIdentifier.Ubuntu1404X64 => "ubuntu.14.04-x64",
            WkHtmlToXRuntimeIdentifier.Ubuntu1404X86 => "ubuntu.14.04-x86",
            WkHtmlToXRuntimeIdentifier.Ubuntu1604X64 => "ubuntu.16.04-x64",
            WkHtmlToXRuntimeIdentifier.Ubuntu1604X86 => "ubuntu.16.04-x86",
            WkHtmlToXRuntimeIdentifier.Ubuntu1804X64 => "ubuntu.18.04-x64",
            WkHtmlToXRuntimeIdentifier.Ubuntu1804X86 => "ubuntu.18.04-x86",
            WkHtmlToXRuntimeIdentifier.Ubuntu2004X64 => "ubuntu.20.04-x64",
            _ => throw new InvalidLinuxRuntimeIdentifierException(),
        };
    }
}
