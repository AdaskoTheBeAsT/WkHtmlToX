using System;
using AdaskoTheBeAsT.Interop.Execution;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.Loaders;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.DependencyInjection.Test;

public sealed class WkHtmlToXServiceCollectionExtensionsTest
{
    [Fact]
    public void AddWkHtmlToXShouldThrowWhenServicesIsNull()
    {
        // Arrange
        const IServiceCollection? services = null;
        var configuration = new WkHtmlToXConfiguration(platformId: 0, runtimeIdentifier: null);

        // Act
        Action action = () => services!.AddWkHtmlToX(configuration);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName(nameof(services));
    }

    [Fact]
    public void AddWkHtmlToXShouldThrowWhenConfigurationIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        const WkHtmlToXConfiguration? configuration = null;

        // Act
        Action action = () => services.AddWkHtmlToX(configuration: configuration!);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName(nameof(configuration));
    }

    [Fact]
    public void AddWkHtmlToXShouldRegisterConfigurationAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new WkHtmlToXConfiguration(platformId: 0, runtimeIdentifier: null);

        // Act
        services.AddWkHtmlToX(configuration);

        // Assert
        using var provider = services.BuildServiceProvider();
        var resolved = provider.GetRequiredService<WkHtmlToXConfiguration>();
        resolved.Should().BeSameAs(configuration);
    }

    [Fact]
    public void AddWkHtmlToXShouldRegisterLibraryLoaderFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new WkHtmlToXConfiguration(platformId: 0, runtimeIdentifier: null);

        // Act
        services.AddWkHtmlToX(configuration);

        // Assert
        using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<ILibraryLoaderFactory>();
        factory.Should().BeOfType<LibraryLoaderFactory>();
    }

    [Fact]
    public void AddWkHtmlToXShouldRegisterSessionFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new WkHtmlToXConfiguration(platformId: 0, runtimeIdentifier: null);

        // Act
        services.AddWkHtmlToX(configuration);

        // Assert
        using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IExecutionSessionFactory<WkHtmlToXSession>>();
        factory.Should().NotBeNull();
    }

    [Fact]
    public void AddWkHtmlToXShouldRegisterExecutionWorker()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new WkHtmlToXConfiguration(platformId: 0, runtimeIdentifier: null);

        // Act
        services.AddWkHtmlToX(configuration);

        // Assert
        using var provider = services.BuildServiceProvider();
        var worker = provider.GetRequiredService<IExecutionWorker<WkHtmlToXSession>>();
        worker.Should().NotBeNull();
    }

    [Fact]
    public void AddWkHtmlToXShouldRegisterEngineAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new WkHtmlToXConfiguration(platformId: 0, runtimeIdentifier: null);

        // Act
        services.AddWkHtmlToX(configuration);

        // Assert
        using var provider = services.BuildServiceProvider();
        var first = provider.GetRequiredService<IWkHtmlToXEngine>();
        var second = provider.GetRequiredService<IWkHtmlToXEngine>();

        using (new AssertionScope())
        {
            first.Should().NotBeNull();
            second.Should().BeSameAs(first);
        }
    }

    [Fact]
    public void AddWkHtmlToXShouldRegisterConverters()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new WkHtmlToXConfiguration(platformId: 0, runtimeIdentifier: null);

        // Act
        services.AddWkHtmlToX(configuration);

        // Assert
        using var provider = services.BuildServiceProvider();
        var pdfConverter = provider.GetRequiredService<IPdfConverter>();
        var imageConverter = provider.GetRequiredService<IImageConverter>();

        using (new AssertionScope())
        {
            pdfConverter.Should().BeOfType<PdfConverter>();
            imageConverter.Should().BeOfType<ImageConverter>();
        }
    }

    [Fact]
    public void AddWkHtmlToXShouldApplyConfigureWorkerDelegate()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new WkHtmlToXConfiguration(platformId: 0, runtimeIdentifier: null);
        const string customName = "custom-worker";
        var invoked = false;

        // Act
        services.AddWkHtmlToX(
            configuration,
            options =>
            {
                options.Name = customName;
                invoked = true;
            });

        // Assert
        using var provider = services.BuildServiceProvider();
        var worker = provider.GetRequiredService<IExecutionWorker<WkHtmlToXSession>>();
        using (new AssertionScope())
        {
            worker.Should().NotBeNull();
            invoked.Should().BeTrue();
        }
    }

    [Fact]
    public void AddWkHtmlToXShouldReturnSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new WkHtmlToXConfiguration(platformId: 0, runtimeIdentifier: null);

        // Act
        var result = services.AddWkHtmlToX(configuration);

        // Assert
        result.Should().BeSameAs(services);
    }
}
