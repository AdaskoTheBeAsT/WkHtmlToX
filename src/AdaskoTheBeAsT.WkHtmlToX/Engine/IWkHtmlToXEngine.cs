using System;
using System.Threading;
using AdaskoTheBeAsT.WkHtmlToX.WorkItems;

namespace AdaskoTheBeAsT.WkHtmlToX.Engine
{
    public interface IWkHtmlToXEngine
        : IDisposable
    {
        void Initialize();

        void AddConvertWorkItem(ConvertWorkItemBase item, CancellationToken cancellationToken);
    }
}
