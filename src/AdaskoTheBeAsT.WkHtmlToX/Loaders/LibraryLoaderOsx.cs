namespace AdaskoTheBeAsT.WkHtmlToX.Loaders
{
    internal sealed class LibraryLoaderOsx
        : LibraryLoaderPosix
    {
        protected override string GetLibraryName() => "libwkhtmltox.0.12.6.dylib";

        protected override string GetRuntimeIdentifier() => $"osx-{GetProcessorArchitecture()}";
    }
}
