using System;
using System.Linq;
using AdaskoTheBeAsT.Interop.Execution;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Hosting.Test;

public sealed class WkHtmlToXHostingServiceCollectionExtensionsTest
{
    [Fact]
    public void AddWkHtmlToXHostedServiceShouldThrowWhenServicesIsNull()
    {
        // Arrange
        IServiceCollection? services = null;
        var configuration = new WkHtmlToXConfiguration(platformId: 0, runtimeIdentifier: null);

        // Act
        Action action = () => services!.AddWkHtmlToXHostedService(configuration);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName(nameof(services));
    }

    [Fact]
    public void AddWkHtmlToXHostedServiceShouldThrowWhenConfigurationIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        WkHtmlToXConfiguration? configuration = null;

        // Act
        Action action = () => services.AddWkHtmlToXHostedService(configuration: configuration!);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName(nameof(configuration));
    }

    [Fact]
    public void AddWkHtmlToXHostedServiceShouldRegisterHostedService()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new WkHtmlToXConfiguration(platformId: 0, runtimeIdentifier: null);

        // Act
        services.AddWkHtmlToXHostedService(configuration);

        // Assert
        using var provider = services.BuildServiceProvider();
        var hostedServices = provider.GetServices<IHostedService>().ToList();
        hostedServices.Should().NotBeEmpty();
    }

    [Fact]
    public void AddWkHtmlToXHostedServiceShouldRegisterConfigurationAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new WkHtmlToXConfiguration(platformId: 0, runtimeIdentifier: null);

        // Act
        services.AddWkHtmlToXHostedService(configuration);

        // Assert
        using var provider = services.BuildServiceProvider();
        var resolved = provider.GetRequiredService<WkHtmlToXConfiguration>();
        resolved.Should().BeSameAs(configuration);
    }

    [Fact]
    public void AddWkHtmlToXHostedServiceShouldRegisterEngineAndConverters()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new WkHtmlToXConfiguration(platformId: 0, runtimeIdentifier: null);

        // Act
        services.AddWkHtmlToXHostedService(configuration);

        // Assert
        using var provider = services.BuildServiceProvider();
        var engine = provider.GetRequiredService<IWkHtmlToXEngine>();
        var pdfConverter = provider.GetRequiredService<IPdfConverter>();
        var imageConverter = provider.GetRequiredService<IImageConverter>();

        using (new AssertionScope())
        {
            engine.Should().NotBeNull();
            pdfConverter.Should().NotBeNull();
            imageConverter.Should().NotBeNull();
        }
    }

    [Fact]
    public void AddWkHtmlToXHostedServiceShouldRegisterExecutionWorker()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new WkHtmlToXConfiguration(platformId: 0, runtimeIdentifier: null);

        // Act
        services.AddWkHtmlToXHostedService(configuration);

        // Assert
        using var provider = services.BuildServiceProvider();
        var worker = provider.GetRequiredService<IExecutionWorker<WkHtmlToXSession>>();
        worker.Should().NotBeNull();
    }

    [Fact]
    public void AddWkHtmlToXHostedServiceShouldApplyConfigureWorkerDelegate()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new WkHtmlToXConfiguration(platformId: 0, runtimeIdentifier: null);
        const string customName = "custom-hosted-worker";

        // Act
        services.AddWkHtmlToXHostedService(
            configuration,
            options => options.Name = customName);

        // Assert
        using var provider = services.BuildServiceProvider();
        var worker = provider.GetRequiredService<IExecutionWorker<WkHtmlToXSession>>();
        worker.Should().NotBeNull();
    }

    [Fact]
    public void AddWkHtmlToXHostedServiceShouldReturnSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new WkHtmlToXConfiguration(platformId: 0, runtimeIdentifier: null);

        // Act
        var result = services.AddWkHtmlToXHostedService(configuration);

        // Assert
        result.Should().BeSameAs(services);
    }
}
