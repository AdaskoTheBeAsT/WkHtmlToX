using System;
using AdaskoTheBeAsT.Interop.Execution;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.Loaders;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
        resolved.Should().NotBeSameAs(configuration);
        resolved.PlatformId.Should().Be(configuration.PlatformId);
        resolved.RequestOptions.Should().NotBeSameAs(configuration.RequestOptions);
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

    [Fact]
    public void AddWkHtmlToXShouldSnapshotWorkerOptionsAtRegistration()
    {
        var services = new ServiceCollection();
        var configuration = new WkHtmlToXConfiguration
        {
            WorkerOptions = new WkHtmlToXWorkerOptions
            {
                Name = "renderer",
                MaxOperationsPerSession = 42,
                DisposeTimeout = TimeSpan.FromSeconds(3),
                ShutdownMode = WkHtmlToXShutdownMode.CancelPending,
            },
        };
        services.AddWkHtmlToX(configuration);
        configuration.WorkerOptions.Name = "changed";
        configuration.WorkerOptions.MaxOperationsPerSession = -1;
        configuration.WorkerOptions.DisposeTimeout = TimeSpan.Zero;
        configuration.WorkerOptions.ShutdownMode = WkHtmlToXShutdownMode.Drain;
        using var provider = services.BuildServiceProvider();
        var resolved = provider.GetRequiredService<WkHtmlToXConfiguration>();
        resolved.WorkerOptions.Should().NotBeSameAs(configuration.WorkerOptions);
        var options = provider.GetRequiredService<IOptionsMonitor<ExecutionWorkerOptions>>()
            .Get(typeof(WkHtmlToXSession).FullName);
        options.Name.Should().Be("renderer");
        options.MaxOperationsPerSession.Should().Be(42);
        options.DisposeTimeout.Should().Be(TimeSpan.FromSeconds(3));
        options.ShutdownMode.Should().Be(ExecutionShutdownMode.CancelPending);
        options.UseStaThread.Should().BeTrue();
        var worker = provider.GetRequiredService<IExecutionWorker<WkHtmlToXSession>>();
        worker.Name.Should().Be("renderer");
    }

    [Fact]
    public void AdvancedWorkerConfigurationShouldOverrideWrapperPolicy()
    {
        var services = new ServiceCollection();
        var configuration = new WkHtmlToXConfiguration
        {
            WorkerOptions = new WkHtmlToXWorkerOptions { Name = "base", MaxOperationsPerSession = 42 },
        };
        services.AddWkHtmlToX(configuration, options =>
        {
            options.Name.Should().Be("base");
            options.MaxOperationsPerSession.Should().Be(42);
            options.Name = "advanced";
            options.MaxOperationsPerSession = 7;
            options.QueueCapacity = 3;
            options.ShutdownMode = ExecutionShutdownMode.CancelPending;
        });
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptionsMonitor<ExecutionWorkerOptions>>()
            .Get(typeof(WkHtmlToXSession).FullName);
        options.Name.Should().Be("advanced");
        options.MaxOperationsPerSession.Should().Be(7);
        options.QueueCapacity.Should().Be(3);
        options.ShutdownMode.Should().Be(ExecutionShutdownMode.CancelPending);
    }

    [Fact]
    public void InvalidWorkerPolicyShouldNotPartiallyRegisterServices()
    {
        var services = new ServiceCollection();
        var configuration = new WkHtmlToXConfiguration
        {
            WorkerOptions = new WkHtmlToXWorkerOptions { MaxOperationsPerSession = -1 },
        };
        Action register = () => services.AddWkHtmlToX(configuration);
        register.Should().Throw<ArgumentException>();
        services.Should().BeEmpty();
    }
}
