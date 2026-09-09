using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Interop.Execution;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Engine;

#pragma warning disable RCS1261 // In-memory streams use synchronous disposal on all TFMs.
public sealed class RequestShutdownTest
{
    [Theory]
    [InlineData(ExecutionShutdownMode.Drain)]
    [InlineData(ExecutionShutdownMode.CancelPending)]
    public async Task ShutdownShouldJoinInputAndCloseAdmissionAsync(ExecutionShutdownMode mode)
    {
        var fixture = new NativeRuntimeTestFixture();
        using var input = new RequestTestStream(gateReads: true);
        var document = NativeRuntimeTestFixture.PdfDocument();
        document.ObjectSettings[0].HtmlContent = null;
        document.ObjectSettings[0].HtmlContentStream = input;
        await using var engine = fixture.CreateEngine(new ExecutionWorkerOptions { ShutdownMode = mode });
        await engine.InitializeAsync(TestContext.Current.CancellationToken);
        var conversion = engine.ConvertPdfAsync(document, _ => Stream.Null, TestContext.Current.CancellationToken);
        try
        {
#pragma warning disable VSTHRD003 // Deterministic input gate.
            await input.Entered.Task;
#pragma warning restore VSTHRD003
            // An uncancellable wait exposes the shared completion task rather than a per-caller wrapper.
            var shutdown = engine.ShutdownAsync(CancellationToken.None);
            var repeated = engine.ShutdownAsync(
                mode == ExecutionShutdownMode.Drain ? ExecutionShutdownMode.CancelPending : ExecutionShutdownMode.Drain,
                CancellationToken.None);
            repeated.Should().BeSameAs(shutdown);
            shutdown.IsCompleted.Should().BeFalse();
            fixture.Pdf.Verify(m => m.Terminate(), Times.Never);
            Func<Task<ConversionResult>> rejected = async () => await engine.ConvertImageAsync(NativeRuntimeTestFixture.ImageDocument(), _ => Stream.Null, TestContext.Current.CancellationToken);
            await rejected.Should().ThrowAsync<ObjectDisposedException>();
            input.Release.TrySetResult(true);
            await AssertOutcomeAsync(conversion, mode);
            await shutdown;
            await repeated;
            input.CanRead.Should().BeTrue();
            fixture.Pdf.Verify(m => m.Terminate(), Times.Once);
            fixture.Pdf.Verify(m => m.Convert(It.IsAny<IntPtr>()), mode == ExecutionShutdownMode.Drain ? Times.Once : Times.Never);
        }
        finally
        {
            input.Release.TrySetResult(true);
        }
    }

    [Theory]
    [InlineData(ExecutionShutdownMode.Drain)]
    [InlineData(ExecutionShutdownMode.CancelPending)]
    public async Task ShutdownShouldHandleQueuedWorkWithoutCancelingActiveNativeCallAsync(ExecutionShutdownMode mode)
    {
        var fixture = new NativeRuntimeTestFixture();
        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        fixture.Pdf.Setup(m => m.Convert(It.IsAny<IntPtr>())).Returns(() =>
        {
            entered.Set();
            release.Wait(TestContext.Current.CancellationToken);
            return true;
        });
        await using var engine = fixture.CreateEngine();
        var active = engine.ConvertPdfAsync(NativeRuntimeTestFixture.PdfDocument(), _ => Stream.Null, TestContext.Current.CancellationToken);
        try
        {
            entered.Wait(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken).Should().BeTrue();
            var queued = engine.ConvertImageAsync(NativeRuntimeTestFixture.ImageDocument(), _ => Stream.Null, TestContext.Current.CancellationToken);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            while (engine.QueueDepth == 0)
            {
                await Task.Delay(10, timeout.Token);
            }

            var shutdown = engine.ShutdownAsync(mode, TestContext.Current.CancellationToken);
            active.IsCompleted.Should().BeFalse();
            shutdown.IsCompleted.Should().BeFalse();
            release.Set();
            (await active).Success.Should().BeTrue();
            await AssertOutcomeAsync(queued, mode);
            await shutdown;
            fixture.Image.Verify(m => m.Convert(It.IsAny<IntPtr>()), mode == ExecutionShutdownMode.Drain ? Times.Once : Times.Never);
            fixture.Pdf.Verify(m => m.Initialize(It.IsAny<int>()), Times.Once);
        }
        finally
        {
            release.Set();
        }
    }

    [Theory]
    [InlineData(ExecutionShutdownMode.Drain)]
    [InlineData(ExecutionShutdownMode.CancelPending)]
    public async Task ShutdownDeadlineShouldNotReleaseActiveDeliveryOrNativeOwnershipAsync(ExecutionShutdownMode mode)
    {
        var ownership = new NativeRuntimeOwnership();
        var fixture = new NativeRuntimeTestFixture(ownership: ownership);
        RequestOwnershipTest.SetOutput(fixture, 4);
        using var output = new RequestTestStream(gateWrites: true);
        using var deadline = new CancellationTokenSource();
        await using var engine = fixture.CreateEngine();
#pragma warning disable IDISP011 // Output stays borrowed until conversion is awaited below.
        var conversion = engine.ConvertPdfAsync(NativeRuntimeTestFixture.PdfDocument(), _ => output, TestContext.Current.CancellationToken);
#pragma warning restore IDISP011
        try
        {
#pragma warning disable VSTHRD003 // Deterministic delivery gate.
            await output.Entered.Task;
#pragma warning restore VSTHRD003
            var wait = engine.ShutdownAsync(mode, deadline.Token);
#if NET8_0_OR_GREATER
            await deadline.CancelAsync();
#else
            deadline.Cancel();
#endif
#pragma warning disable VSTHRD003 // Observe only the canceled shutdown wait, not actual completion.
            Func<Task> action = async () => await wait;
#pragma warning restore VSTHRD003
            await action.Should().ThrowAsync<OperationCanceledException>();
            var shutdown = engine.ShutdownAsync(TestContext.Current.CancellationToken);
            shutdown.IsCompleted.Should().BeFalse();
            conversion.IsCompleted.Should().BeFalse();
            fixture.Pdf.Verify(m => m.Terminate(), Times.Never);
            Action replacement = () =>
            {
                var other = new NativeRuntimeTestFixture(ownership: ownership);
                using var worker = new WkHtmlToXWorker(other.Factory, new ExecutionWorkerOptions());
            };
            replacement.Should().Throw<InvalidOperationException>();
            output.Release.TrySetResult(true);
            (await conversion).Success.Should().BeTrue();
            await shutdown;
            output.CanWrite.Should().BeTrue();
            fixture.Pdf.Verify(m => m.Terminate(), Times.Once);
        }
        finally
        {
            output.Release.TrySetResult(true);
        }
    }

    [Fact]
    public async Task WorkerDisposalAfterSynchronousTimeoutMustNotBypassInputDrainAsync()
    {
        var fixture = new NativeRuntimeTestFixture();
        using var input = new RequestTestStream(gateReads: true);
        await using var worker = new WkHtmlToXWorker(fixture.Factory, new ExecutionWorkerOptions { DisposeTimeout = TimeSpan.Zero });
        await using var engine = new WkHtmlToXEngine(worker);
        var document = NativeRuntimeTestFixture.PdfDocument();
        document.ObjectSettings[0].HtmlContent = null;
        document.ObjectSettings[0].HtmlContentStream = input;
        await engine.InitializeAsync(TestContext.Current.CancellationToken);
        var conversion = engine.ConvertPdfAsync(document, _ => Stream.Null, TestContext.Current.CancellationToken);
        try
        {
#pragma warning disable VSTHRD003 // Deterministic input gate.
            await input.Entered.Task;
#pragma warning restore VSTHRD003
#pragma warning disable IDISP016, MA0042, VSTHRD103, S6966, S5034 // Reproduce synchronous service-provider disposal after a zero timeout.
            engine.Dispose();
            worker.Dispose();
#pragma warning restore IDISP016, MA0042, VSTHRD103, S6966, S5034
            fixture.Pdf.Verify(m => m.Terminate(), Times.Never);
            input.Release.TrySetResult(true);
            (await conversion).Success.Should().BeTrue();
            await engine.ShutdownAsync(TestContext.Current.CancellationToken);
            fixture.Pdf.Verify(m => m.Terminate(), Times.Once);
        }
        finally
        {
            input.Release.TrySetResult(true);
        }
    }

    [Fact]
    public async Task PreCanceledShutdownWaitShouldStillCloseAdmissionAndDrainInputAsync()
    {
        var fixture = new NativeRuntimeTestFixture();
        using var input = new RequestTestStream(gateReads: true);
        var document = NativeRuntimeTestFixture.PdfDocument();
        document.ObjectSettings[0].HtmlContent = null;
        document.ObjectSettings[0].HtmlContentStream = input;
        await using var engine = fixture.CreateEngine();
        var conversion = engine.ConvertPdfAsync(document, _ => Stream.Null, TestContext.Current.CancellationToken);
        try
        {
#pragma warning disable VSTHRD003 // Explicit input gate.
            await input.Entered.Task;
#pragma warning restore VSTHRD003
            Func<Task> stopping = async () => await engine.ShutdownAsync(new CancellationToken(canceled: true));
            await stopping.Should().ThrowAsync<OperationCanceledException>();
            Func<Task<ConversionResult>> rejected = async () => await engine.ConvertImageAsync(
                NativeRuntimeTestFixture.ImageDocument(), _ => Stream.Null, TestContext.Current.CancellationToken);
            await rejected.Should().ThrowAsync<ObjectDisposedException>();
            input.Release.TrySetResult(true);
            (await conversion).Success.Should().BeTrue();
            await engine.ShutdownAsync(TestContext.Current.CancellationToken);
        }
        finally
        {
            input.Release.TrySetResult(true);
        }
    }

    private static async Task AssertOutcomeAsync(Task<ConversionResult> conversion, ExecutionShutdownMode mode)
    {
#pragma warning disable VSTHRD003 // Caller owns and releases the deterministic request gates.
        if (mode == ExecutionShutdownMode.Drain)
        {
            (await conversion).Success.Should().BeTrue();
        }
        else
        {
            Func<Task> action = async () => await conversion;
            await action.Should().ThrowAsync<OperationCanceledException>();
        }
#pragma warning restore VSTHRD003
    }
}
#pragma warning restore RCS1261
