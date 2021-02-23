using AdaskoTheBeAsT.WkHtmlToX.Engine;

namespace AdaskoTheBeAsT.WkHtmlToX.Abstractions
{
    internal interface ILibraryLoaderFactory
    {
        ILibraryLoader Create(WkHtmlToXConfiguration wkHtmlToXConfiguration);
    }
}
