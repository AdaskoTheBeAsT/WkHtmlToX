using System;
using AdaskoTheBeAsT.Interop.Execution;
using AdaskoTheBeAsT.WkHtmlToX.DependencyInjection;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using Microsoft.Extensions.DependencyInjection;

namespace AdaskoTheBeAsT.WkHtmlToX.Hosting;

/// <summary>
/// <see cref="IServiceCollection"/> extensions that register the WkHtmlToX
/// engine, converters and an <c>IHostedService</c> wrapper that drives the
/// whole engine pipeline lifecycle through the generic host.
/// </summary>
public static class WkHtmlToXHostingServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IWkHtmlToXEngine"/>, <see cref="Abstractions.IPdfConverter"/>,
    /// <see cref="Abstractions.IImageConverter"/>, the underlying
    /// <see cref="IExecutionWorker{TSession}"/> and a hosted service wrapper
    /// that initializes the engine on start and joins its pipeline on stop.
    /// </summary>
    /// <param name="services">The service collection to mutate.</param>
    /// <param name="configuration">WkHtmlToX runtime configuration.</param>
    /// <param name="configureWorker">Optional worker options configuration delegate.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="services"/> or <paramref name="configuration"/> is <see langword="null"/>.
    /// </exception>
    public static IServiceCollection AddWkHtmlToXHostedService(
        this IServiceCollection services,
        WkHtmlToXConfiguration configuration,
        Action<ExecutionWorkerOptions>? configureWorker = null)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
#else
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }
#endif

        services.AddWkHtmlToX(configuration, configureWorker);
        services.AddHostedService<WkHtmlToXHostedService>();

        return services;
    }
}
