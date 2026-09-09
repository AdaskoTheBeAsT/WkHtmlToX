using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.Hosting;
using AdaskoTheBeAsT.WkHtmlToX.Settings;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.IntegrationTest;

public sealed class NativeLifecycleTest
{
    [Fact]
    public async Task HostedMixedConversionsShouldShareThreadAndStopCleanlyAsync()
    {
        var threads = new ConcurrentBag<int>();
        var configuration = new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, runtimeIdentifier: null)
        {
            ProgressChangedAction = _ => threads.Add(Environment.CurrentManagedThreadId),
            WorkerOptions = new WkHtmlToXWorkerOptions { MaxOperationsPerSession = 1 },
        };
        using var host = new HostBuilder()
            .ConfigureServices(services => services.AddWkHtmlToXHostedService(
                configuration,
                options => options.QueueCapacity = 16))
            .Build();
        await host.StartAsync(TestContext.Current.CancellationToken);
        var engine = host.Services.GetRequiredService<IWkHtmlToXAsyncEngine>();
        var renders = Enumerable.Range(0, 6).Select(async index =>
        {
#pragma warning disable RCS1261 // MemoryStream stays synchronous for net472 targets.
            using var output = new MemoryStream();
#pragma warning restore RCS1261
#pragma warning disable IDISP011 // The engine borrows the caller-owned stream only during conversion.
            var result = index % 2 == 0
                ? await engine.ConvertPdfAsync(PdfDocument(), _ => output, TestContext.Current.CancellationToken)
                : await engine.ConvertImageAsync(ImageDocument(), _ => output, TestContext.Current.CancellationToken);
#pragma warning restore IDISP011
            result.Success.Should().BeTrue();
            output.Length.Should().BePositive();
            output.CanWrite.Should().BeTrue();
        });
        await Task.WhenAll(renders);
        await host.StopAsync(TestContext.Current.CancellationToken);
        engine.IsFaulted.Should().BeFalse();
        threads.Should().NotBeEmpty();
        threads.Distinct().Should().ContainSingle();

        // The old service provider is still alive, but native teardown has released its lease.
        await using var replacement = new WkHtmlToXEngine(configuration);
        await replacement.InitializeAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CallbackFailureShouldBeReportedAfterNativeConversionAndAllowNextRequestAsync()
    {
        var failCallback = 1;
        var configuration = new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, runtimeIdentifier: null)
        {
            ProgressChangedAction = _ =>
            {
                if (Interlocked.Exchange(ref failCallback, 0) == 1)
                {
                    throw new InvalidOperationException("test subscriber");
                }
            },
        };
        await using var engine = new WkHtmlToXEngine(configuration);
#pragma warning disable RCS1261 // MemoryStream stays synchronous for net472 targets.
        using var firstOutput = new MemoryStream();
#pragma warning restore RCS1261
#pragma warning disable IDISP011, RCS1261 // Caller-owned streams borrowed only during conversion.
        var first = await engine.ConvertPdfAsync(PdfDocument(), _ => firstOutput, TestContext.Current.CancellationToken);
        first.FailureKind.Should().Be(ConversionFailureKind.CallbackError);
        first.CallbackException.Should().BeOfType<InvalidOperationException>();
        firstOutput.Length.Should().BePositive();
        using var secondOutput = new MemoryStream();
        var second = await engine.ConvertImageAsync(ImageDocument(), _ => secondOutput, TestContext.Current.CancellationToken);
#pragma warning restore IDISP011, RCS1261
        second.Success.Should().BeTrue();
        engine.IsFaulted.Should().BeFalse();
    }

    [Fact]
    public async Task OutputFactoryFailureShouldNotPreventAnotherNativeConversionAsync()
    {
        await using var engine = new WkHtmlToXEngine(
            new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, runtimeIdentifier: null));
        var first = await engine.ConvertPdfAsync(
            PdfDocument(),
            _ => throw new InvalidOperationException("destination factory"),
            TestContext.Current.CancellationToken);
        first.FailureKind.Should().Be(ConversionFailureKind.OutputWriteError);
        first.Exception.Should().BeOfType<InvalidOperationException>();
#pragma warning disable RCS1261 // MemoryStream stays synchronous for net472 targets.
        using var output = new MemoryStream();
#pragma warning restore RCS1261
#pragma warning disable IDISP011, RCS1261 // Caller-owned stream borrowed only during conversion.
        (await engine.ConvertPdfAsync(PdfDocument(), _ => output, TestContext.Current.CancellationToken))
            .Success.Should().BeTrue();
#pragma warning restore IDISP011, RCS1261
    }

    [Fact]
    public async Task HostDeadlineShouldLeaveDeliveryOwnedUntilPipelineExitAsync()
    {
        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        using var deadline = new CancellationTokenSource();
        using var host = new HostBuilder()
            .ConfigureServices(services => services.AddWkHtmlToXHostedService(new WkHtmlToXConfiguration()))
            .Build();
        await host.StartAsync(TestContext.Current.CancellationToken);
        var engine = host.Services.GetRequiredService<IWkHtmlToXAsyncEngine>();
        var conversion = engine.ConvertPdfAsync(
            PdfDocument(),
            _ =>
            {
                entered.Set();
                release.Wait(TestContext.Current.CancellationToken);
                return Stream.Null;
            },
            TestContext.Current.CancellationToken);
        try
        {
            entered.Wait(TimeSpan.FromSeconds(20), TestContext.Current.CancellationToken).Should().BeTrue();
            var stopping = host.StopAsync(deadline.Token);
#if NET8_0_OR_GREATER
            await deadline.CancelAsync();
#else
            deadline.Cancel();
#endif
#pragma warning disable VSTHRD003 // Only the host wait is canceled; conversion remains blocked at its factory.
            Func<Task> action = async () => await stopping;
#pragma warning restore VSTHRD003
            await action.Should().ThrowAsync<OperationCanceledException>();
            conversion.IsCompleted.Should().BeFalse();
            Action replacement = () =>
            {
                using var second = new WkHtmlToXEngine(new WkHtmlToXConfiguration());
            };
            replacement.Should().Throw<InvalidOperationException>();
            release.Set();
            (await conversion).Success.Should().BeTrue();
            await engine.ShutdownAsync(TestContext.Current.CancellationToken);
        }
        finally
        {
            release.Set();
        }
    }

    private static HtmlToPdfDocument PdfDocument() =>
        new()
        {
            ObjectSettings =
            {
                new PdfObjectSettings { HtmlContent = "<html><body><p>Native lifecycle test</p></body></html>" },
            },
        };

    private static HtmlToImageDocument ImageDocument() =>
        new() { ImageSettings = new ImageSettings { In = "about:blank", Format = "png" } };
}
