using AdaskoTheBeAsT.WkHtmlToX.Loaders;

namespace AdaskoTheBeAsT.WkHtmlToX.Abstractions
{
    public interface ILibraryLoaderFactory
    {
        ILibraryLoader Create(int platformId, WkHtmlToXRuntimeIdentifier? runtimeIdentifier);
    }
}
