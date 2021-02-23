using System;

namespace AdaskoTheBeAsT.WkHtmlToX.Abstractions
{
    internal interface ILibraryLoader : IDisposable
    {
        void Load();

        void Release();
    }
}
