using System;
using System.Threading;
using AdaskoTheBeAsT.WkHtmlToX.WorkItems;

namespace AdaskoTheBeAsT.WkHtmlToX.Engine;

public interface IWkHtmlToXEngine
    : IDisposable
{
    void Initialize();

#pragma warning disable S1133 // Planned removal in the next major release; retained for migration.
    [Obsolete("Use IWkHtmlToXAsyncEngine.ConvertPdfAsync or ConvertImageAsync instead.")]
#pragma warning restore S1133
    void AddConvertWorkItem(ConvertWorkItemBase item, CancellationToken cancellationToken);
}
