using AdaskoTheBeAsT.WkHtmlToX.Loaders;

namespace AdaskoTheBeAsT.WkHtmlToX.Abstractions
{
    public interface ILibraryLoaderFactory
    {
        ILibraryLoader Create(WkHtmlToXRuntimeIdentifier? runtimeIdentifier);
    }
}
