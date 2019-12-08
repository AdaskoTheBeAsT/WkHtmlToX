namespace AdaskoTheBeAsT.WkHtmlToX.Loaders
{
    public class LibraryLoaderOsx
        : LibraryLoaderPosix
    {
        protected override string GetLibraryName() => "libwkhtmltox.dylib";

        protected override string GetRuntimeIdentifier() => $"osx-{GetProcessorArchitecture()}";
    }
}
