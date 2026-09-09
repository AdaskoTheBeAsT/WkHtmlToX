using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Interop.Execution;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Engine;

public sealed class WorkerOptionsTest
{
    [Fact]
    public void DefaultsShouldPreserveWorkerPolicy()
    {
        var options = new WkHtmlToXWorkerOptions();
        var execution = new ExecutionWorkerOptions();
        options.Snapshot().ApplyTo(execution);
        execution.Name.Should().Be("WkHtmlToX Engine Worker");
        execution.UseStaThread.Should().BeTrue();
        execution.MaxOperationsPerSession.Should().Be(0);
        execution.DisposeTimeout.Should().Be(Timeout.InfiniteTimeSpan);
        execution.ShutdownMode.Should().Be(ExecutionShutdownMode.Drain);
        execution.QueueCapacity.Should().Be(0);
    }

    [Theory]
    [InlineData(WkHtmlToXShutdownMode.Drain, ExecutionShutdownMode.Drain)]
    [InlineData(WkHtmlToXShutdownMode.CancelPending, ExecutionShutdownMode.CancelPending)]
    public void SnapshotShouldDetachAndMapWorkerPolicy(WkHtmlToXShutdownMode mode, ExecutionShutdownMode expected)
    {
        var configuration = new WkHtmlToXConfiguration
        {
            WorkerOptions = new WkHtmlToXWorkerOptions
            {
                Name = "renderer",
                MaxOperationsPerSession = 123,
                DisposeTimeout = TimeSpan.FromSeconds(2),
                ShutdownMode = mode,
            },
        };
        var snapshot = configuration.Snapshot();
        configuration.WorkerOptions.Name = "changed";
        configuration.WorkerOptions.MaxOperationsPerSession = -1;
        configuration.WorkerOptions.DisposeTimeout = TimeSpan.Zero;
        configuration.WorkerOptions.ShutdownMode = (WkHtmlToXShutdownMode)(-1);
        var execution = new ExecutionWorkerOptions();
        snapshot.WorkerOptions.ApplyTo(execution);
        snapshot.WorkerOptions.Should().NotBeSameAs(configuration.WorkerOptions);
        execution.Name.Should().Be("renderer");
        execution.MaxOperationsPerSession.Should().Be(123);
        execution.DisposeTimeout.Should().Be(TimeSpan.FromSeconds(2));
        execution.ShutdownMode.Should().Be(expected);
        execution.UseStaThread.Should().BeTrue();
    }

    [Theory]
    [InlineData(-1, -1, 0, nameof(WkHtmlToXWorkerOptions.MaxOperationsPerSession))]
    [InlineData(0, -2, 0, nameof(WkHtmlToXWorkerOptions.DisposeTimeout))]
    [InlineData(0, 2147483648L, 0, nameof(WkHtmlToXWorkerOptions.DisposeTimeout))]
    [InlineData(0, -1, -1, nameof(WkHtmlToXWorkerOptions.ShutdownMode))]
    [InlineData(0, -1, 2, nameof(WkHtmlToXWorkerOptions.ShutdownMode))]
    public void InvalidPolicyShouldFailBeforeReservingNativeOwnership(int interval, long timeout, int mode, string parameterName)
    {
        var configuration = new WkHtmlToXConfiguration
        {
            WorkerOptions = new WkHtmlToXWorkerOptions
            {
                MaxOperationsPerSession = interval,
                DisposeTimeout = TimeSpan.FromMilliseconds(timeout),
                ShutdownMode = (WkHtmlToXShutdownMode)mode,
            },
        };
        Action create = () =>
        {
            using var invalid = new WkHtmlToXEngine(configuration);
        };
        create.Should().Throw<ArgumentOutOfRangeException>().WithParameterName(parameterName);
        using var valid = new WkHtmlToXEngine(new WkHtmlToXConfiguration());
        valid.IsFaulted.Should().BeFalse();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(int.MaxValue)]
    public void ValidTimeoutBoundariesShouldBeAccepted(int milliseconds)
    {
        var options = new WkHtmlToXWorkerOptions
        {
            MaxOperationsPerSession = int.MaxValue,
            DisposeTimeout = TimeSpan.FromMilliseconds(milliseconds),
        };
        var snapshot = options.Snapshot();
        snapshot.DisposeTimeout.Should().Be(options.DisposeTimeout);
        snapshot.MaxOperationsPerSession.Should().Be(int.MaxValue);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void NullOptionsShouldFailBeforeReservingNativeOwnership(bool workerOptions)
    {
        var configuration = new WkHtmlToXConfiguration();
        if (workerOptions)
        {
            configuration.WorkerOptions = null!;
        }
        else
        {
            configuration.RequestOptions = null!;
        }

        Action create = () =>
        {
            using var invalid = new WkHtmlToXEngine(configuration);
        };
        create.Should().Throw<ArgumentException>();
        using var valid = new WkHtmlToXEngine(new WkHtmlToXConfiguration());
        valid.IsFaulted.Should().BeFalse();
    }

    [Fact]
    public async Task StandaloneEngineShouldUseSnapshottedShutdownPolicyAsync()
    {
        var configuration = new WkHtmlToXConfiguration
        {
            WorkerOptions = new WkHtmlToXWorkerOptions { ShutdownMode = WkHtmlToXShutdownMode.CancelPending },
        };
        await using var engine = new WkHtmlToXEngine(configuration);
        configuration.WorkerOptions.ShutdownMode = WkHtmlToXShutdownMode.Drain;
#if NET8_0_OR_GREATER
        await using var input = new RequestTestStream(gateReads: true);
#else
        using var input = new RequestTestStream(gateReads: true);
#endif
        var document = NativeRuntimeTestFixture.PdfDocument();
        document.ObjectSettings[0].HtmlContent = null;
        document.ObjectSettings[0].HtmlContentStream = input;
        var conversion = engine.ConvertPdfAsync(document, _ => Stream.Null, TestContext.Current.CancellationToken);
        try
        {
#pragma warning disable VSTHRD003 // The test owns this bounded input gate and releases it in finally.
            var entered = await Task.WhenAny(input.Entered.Task, Task.Delay(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken));
#pragma warning restore VSTHRD003
            entered.Should().BeSameAs(input.Entered.Task);
            var shutdown = engine.ShutdownAsync(TestContext.Current.CancellationToken);
            shutdown.IsCompleted.Should().BeFalse();
            input.Release.TrySetResult(true);
#pragma warning disable VSTHRD003 // The input gate is released above before observing the request.
            Func<Task> observe = async () => await conversion;
#pragma warning restore VSTHRD003
            await observe.Should().ThrowAsync<OperationCanceledException>();
            await shutdown;
            engine.IsFaulted.Should().BeFalse();
            input.CanRead.Should().BeTrue();
        }
        finally
        {
            input.Release.TrySetResult(true);
        }
    }
}
