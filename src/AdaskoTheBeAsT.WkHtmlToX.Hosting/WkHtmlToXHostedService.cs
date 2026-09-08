using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using Microsoft.Extensions.Hosting;

namespace AdaskoTheBeAsT.WkHtmlToX.Hosting;

internal sealed class WkHtmlToXHostedService(IWkHtmlToXAsyncEngine engine) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken) => engine.InitializeAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => engine.ShutdownAsync(cancellationToken);
}
