namespace AdaskoTheBeAsT.WkHtmlToX.Loaders
{
    public class LibraryLoaderLinux
        : LibraryLoaderPosix
    {
        protected override string GetLibraryName() => "libwkhtmltox.so";

        protected override string GetRuntimeIdentifier() => $"debian.8-{GetProcessorArchitecture()}";
    }
}
