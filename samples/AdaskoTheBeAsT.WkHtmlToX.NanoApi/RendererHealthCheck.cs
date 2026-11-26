using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AdaskoTheBeAsT.WkHtmlToX.NanoApi;

internal sealed class RendererHealthCheck(IWkHtmlToXAsyncEngine engine) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(engine.IsFaulted
            ? HealthCheckResult.Unhealthy("The native PDF renderer has faulted.")
            : HealthCheckResult.Healthy());
}
