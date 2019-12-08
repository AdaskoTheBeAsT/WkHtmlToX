using System;

namespace AdaskoTheBeAsT.WkHtmlToX.Abstractions
{
    public interface ILibraryLoader : IDisposable
    {
        void Load();

        void Release();
    }
}
