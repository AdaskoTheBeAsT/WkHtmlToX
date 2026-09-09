using System;
using AdaskoTheBeAsT.Interop.Execution;
using AdaskoTheBeAsT.Interop.Execution.DependencyInjection;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.Loaders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace AdaskoTheBeAsT.WkHtmlToX.DependencyInjection;

/// <summary>
/// <see cref="IServiceCollection"/> extensions that register the WkHtmlToX
/// engine, converters and the underlying <see cref="IExecutionWorker{TSession}"/>
/// without hooking into <c>Microsoft.Extensions.Hosting.IHostedService</c>.
/// Consumers that want generic-host driven start/stop should use the
/// <c>AdaskoTheBeAsT.WkHtmlToX.Hosting</c> package instead.
/// </summary>
public static class WkHtmlToXServiceCollectionExtensions
{
    /// <summary>
    /// Registers the engine and underlying worker as singletons.
    /// <see cref="IPdfConverter"/> and <see cref="IImageConverter"/> are transient
    /// facades sharing that one engine.
    /// </summary>
    /// <param name="services">The service collection to mutate.</param>
    /// <param name="configuration">WkHtmlToX runtime configuration.</param>
    /// <param name="configureWorker">Optional advanced Interop configuration, applied after the snapshotted configuration.WorkerOptions.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="services"/> or <paramref name="configuration"/> is <see langword="null"/>.
    /// </exception>
    public static IServiceCollection AddWkHtmlToX(
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

        var snapshot = configuration.Snapshot();
        RegisterCoreServices(services, snapshot);

        services.AddExecutionWorker<WkHtmlToXSession>(options =>
        {
            snapshot.WorkerOptions.ApplyTo(options);
            configureWorker?.Invoke(options);
        });

        RegisterEngineAndConverters(services);

        return services;
    }

    internal static void RegisterCoreServices(
        IServiceCollection services,
        WkHtmlToXConfiguration configuration)
    {
        services.TryAddSingleton(configuration.Snapshot());
        services.TryAddSingleton<ILibraryLoaderFactory, LibraryLoaderFactory>();
        services.TryAddSingleton<IExecutionSessionFactory<WkHtmlToXSession>>(sp =>
            new WkHtmlToXSessionFactory(
                sp.GetRequiredService<WkHtmlToXConfiguration>().Snapshot(),
                sp.GetRequiredService<ILibraryLoaderFactory>()));
        services.TryAddSingleton<IExecutionWorker<WkHtmlToXSession>>(sp =>
            new WkHtmlToXWorker(
                sp.GetRequiredService<IExecutionSessionFactory<WkHtmlToXSession>>(),
                sp.GetRequiredService<IOptionsMonitor<ExecutionWorkerOptions>>()
                    .Get(typeof(WkHtmlToXSession).FullName)));
    }

    internal static void RegisterEngineAndConverters(IServiceCollection services)
    {
        services.TryAddSingleton<IWkHtmlToXEngine>(sp =>
            new WkHtmlToXEngine(
                sp.GetRequiredService<IExecutionWorker<WkHtmlToXSession>>(),
                ownsWorker: true,
                sp.GetRequiredService<WkHtmlToXConfiguration>().RequestOptions));
        services.TryAddSingleton<IWkHtmlToXAsyncEngine>(sp =>
            (IWkHtmlToXAsyncEngine)sp.GetRequiredService<IWkHtmlToXEngine>());

        services.TryAddTransient<IPdfConverter, PdfConverter>();
        services.TryAddTransient<IImageConverter, ImageConverter>();
    }
}
