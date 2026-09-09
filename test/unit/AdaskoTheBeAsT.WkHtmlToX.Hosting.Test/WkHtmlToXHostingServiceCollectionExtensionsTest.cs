using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Interop.Execution;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Hosting.Test;

public sealed class WkHtmlToXHostingServiceCollectionExtensionsTest
{
    [Fact]
    public void AddWkHtmlToXHostedServiceShouldThrowWhenServicesIsNull()
    {
        // Arrange
        const IServiceCollection? services = null;
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
        const WkHtmlToXConfiguration? configuration = null;

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
        hostedServices.Should().ContainSingle().Which.Should().BeOfType<WkHtmlToXHostedService>();
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
        resolved.Should().NotBeSameAs(configuration);
        resolved.PlatformId.Should().Be(configuration.PlatformId);
        resolved.RequestOptions.Should().NotBeSameAs(configuration.RequestOptions);
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
        var invoked = false;

        // Act
        services.AddWkHtmlToXHostedService(
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

    [Fact]
    public async Task HostedLifecycleShouldDriveEngineAndForwardWaitTokensAsync()
    {
        var engine = new Mock<IWkHtmlToXAsyncEngine>(MockBehavior.Strict);
        var token = TestContext.Current.CancellationToken;
        engine.Setup(e => e.InitializeAsync(token)).Returns(Task.CompletedTask);
        engine.Setup(e => e.ShutdownAsync(token)).Returns(Task.CompletedTask);
        var hosted = new WkHtmlToXHostedService(engine.Object);
        await hosted.StartAsync(token);
        await hosted.StopAsync(token);
        engine.Verify(e => e.InitializeAsync(token), Times.Once);
        engine.Verify(e => e.ShutdownAsync(token), Times.Once);
    }

    [Fact]
    public async Task ExpiredHostTokenShouldStillReachEngineShutdownAsync()
    {
        var engine = new Mock<IWkHtmlToXAsyncEngine>(MockBehavior.Strict);
        var token = new CancellationToken(canceled: true);
        engine.Setup(e => e.ShutdownAsync(token)).Returns(Task.FromCanceled(token));
        var hosted = new WkHtmlToXHostedService(engine.Object);
        Func<Task> action = () => hosted.StopAsync(token);
        await action.Should().ThrowAsync<OperationCanceledException>();
        engine.Verify(e => e.ShutdownAsync(token), Times.Once);
    }

    [Fact]
    public void HostedRegistrationShouldSnapshotWrapperWorkerPolicy()
    {
        var services = new ServiceCollection();
        var configuration = new WkHtmlToXConfiguration
        {
            WorkerOptions = new WkHtmlToXWorkerOptions
            {
                Name = "hosted-renderer",
                MaxOperationsPerSession = 500,
                DisposeTimeout = TimeSpan.FromSeconds(5),
                ShutdownMode = WkHtmlToXShutdownMode.CancelPending,
            },
        };
        services.AddWkHtmlToXHostedService(configuration);
        configuration.WorkerOptions = new WkHtmlToXWorkerOptions();
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptionsMonitor<ExecutionWorkerOptions>>()
            .Get(typeof(WkHtmlToXSession).FullName);
        options.Name.Should().Be("hosted-renderer");
        options.MaxOperationsPerSession.Should().Be(500);
        options.DisposeTimeout.Should().Be(TimeSpan.FromSeconds(5));
        options.ShutdownMode.Should().Be(ExecutionShutdownMode.CancelPending);
        options.UseStaThread.Should().BeTrue();
    }
}
