using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Interop.Execution;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.Utils;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Engine;

public sealed class AdapterCorrectnessTest
{
    [Fact]
    public void SecondOwnerShouldBeRejectedBeforeLoadingNativeCode()
    {
        var ownership = new NativeRuntimeOwnership();
        var first = new NativeRuntimeTestFixture(ownership: ownership);
        var second = new NativeRuntimeTestFixture(ownership: ownership);
        using var engine = first.CreateEngine();
        Action create = () =>
        {
            using var rejected = second.CreateEngine();
        };
        create.Should().Throw<InvalidOperationException>();
        second.Loader.Verify(l => l.Load(), Times.Never);
    }

    [Fact]
    public async Task LeaseShouldSurviveRecyclingAndReleaseAfterExitAsync()
    {
        var ownership = new NativeRuntimeOwnership();
        var first = new NativeRuntimeTestFixture(ownership: ownership);
        var second = new NativeRuntimeTestFixture(ownership: ownership);
        await using (var engine = first.CreateEngine(new ExecutionWorkerOptions { MaxOperationsPerSession = 1 }))
        {
            (await engine.ConvertPdfAsync(NativeRuntimeTestFixture.PdfDocument(), _ => Stream.Null, TestContext.Current.CancellationToken))
                .Success.Should().BeTrue();
            first.Pdf.Verify(m => m.Terminate(), Times.Once);
            Action create = () =>
            {
                using var rejected = second.CreateEngine();
            };
            create.Should().Throw<InvalidOperationException>();
        }

        await using var replacement = second.CreateEngine();
        await replacement.InitializeAsync(TestContext.Current.CancellationToken);
        replacement.IsFaulted.Should().BeFalse();
    }

    [Fact]
    public async Task RepeatedDisposalShouldRetainLeaseUntilRunningWorkStopsAsync()
    {
        var ownership = new NativeRuntimeOwnership();
        var fixture = new NativeRuntimeTestFixture(ownership: ownership);
        var competitor = new NativeRuntimeTestFixture(ownership: ownership);
        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        fixture.Pdf.Setup(m => m.Convert(It.IsAny<IntPtr>())).Returns(() =>
        {
            entered.Set();
            release.Wait(TestContext.Current.CancellationToken);
            return true;
        });
        await using var engine = fixture.CreateEngine(new ExecutionWorkerOptions { DisposeTimeout = TimeSpan.Zero });
        var conversion = engine.ConvertPdfAsync(NativeRuntimeTestFixture.PdfDocument(), _ => Stream.Null, TestContext.Current.CancellationToken);
        try
        {
            entered.Wait(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken).Should().BeTrue();
#pragma warning disable IDISP016, MA0042, VSTHRD103, S6966, S5034 // Exercise timeout then distinct async disposal calls.
            engine.Dispose();
            var first = engine.DisposeAsync().AsTask();
            var second = engine.DisposeAsync().AsTask();
#pragma warning restore IDISP016, MA0042, VSTHRD103, S6966, S5034
            first.IsCompleted.Should().BeFalse();
            second.IsCompleted.Should().BeFalse();
            Action create = () =>
            {
                using var rejected = competitor.CreateEngine();
            };
            create.Should().Throw<InvalidOperationException>();
            release.Set();
            await Task.WhenAll(first, second);
            (await conversion).Success.Should().BeTrue();
            fixture.Loader.Verify(l => l.Dispose(), Times.Once);
        }
        finally
        {
            release.Set();
        }

        await using var replacement = competitor.CreateEngine();
    }

    [Fact]
    public async Task MixedFacadesShouldUseOneThreadThroughTeardownAsync()
    {
        var threads = new ConcurrentBag<int>();
        var fixture = new NativeRuntimeTestFixture();
        fixture.Pdf.Setup(m => m.Initialize(It.IsAny<int>())).Returns(() =>
        {
            threads.Add(Environment.CurrentManagedThreadId);
            return 1;
        });
        fixture.Pdf.Setup(m => m.Convert(It.IsAny<IntPtr>())).Returns(() =>
        {
            threads.Add(Environment.CurrentManagedThreadId);
            return true;
        });
        fixture.Image.Setup(m => m.Convert(It.IsAny<IntPtr>())).Returns(() =>
        {
            threads.Add(Environment.CurrentManagedThreadId);
            return true;
        });
        fixture.Image.Setup(m => m.Terminate()).Returns(() =>
        {
            threads.Add(Environment.CurrentManagedThreadId);
            return 1;
        });
        await using (var engine = fixture.CreateEngine())
        {
            var pdf = new PdfConverter(engine);
            var image = new ImageConverter(engine);
            var tasks = Enumerable.Range(0, 20).Select(async index => index % 2 == 0
                ? await pdf.ConvertAsync(NativeRuntimeTestFixture.PdfDocument(), _ => Stream.Null, TestContext.Current.CancellationToken)
                : await image.ConvertAsync(NativeRuntimeTestFixture.ImageDocument(), _ => Stream.Null, TestContext.Current.CancellationToken));
            (await Task.WhenAll(tasks)).Should().OnlyContain(result => result);
        }

        threads.Should().HaveCount(22);
        threads.Distinct().Should().ContainSingle();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task TeardownFailureShouldAttemptEveryStageAndPoisonOwnershipAsync(bool throwException)
    {
        var ownership = new NativeRuntimeOwnership();
        var fixture = new NativeRuntimeTestFixture(ownership: ownership);
        if (throwException)
        {
            fixture.Image.Setup(m => m.Terminate()).Throws(new InvalidOperationException("teardown"));
        }
        else
        {
            fixture.Image.Setup(m => m.Terminate()).Returns(0);
        }

        await using var engine = fixture.CreateEngine();
        await engine.InitializeAsync(TestContext.Current.CancellationToken);
#pragma warning disable IDISP016 // Terminal health remains observable after teardown.
        await engine.DisposeAsync();
        engine.IsFaulted.Should().BeTrue();
        engine.Fault.Should().BeOfType<AggregateException>();
#pragma warning restore IDISP016
        fixture.Pdf.Verify(m => m.Terminate(), Times.Once);
        fixture.Loader.Verify(l => l.Dispose(), Times.Once);
        var second = new NativeRuntimeTestFixture(ownership: ownership);
        Action create = () =>
        {
            using var rejected = second.CreateEngine();
        };
        create.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ConversionErrorShouldKeepSessionAndCaptureHttpStatusAsync(bool image)
    {
        var fixture = new NativeRuntimeTestFixture();
        fixture.Pdf.Setup(m => m.Convert(It.IsAny<IntPtr>())).Returns(value: false);
        fixture.Image.Setup(m => m.Convert(It.IsAny<IntPtr>())).Returns(value: false);
        fixture.Pdf.Setup(m => m.GetHttpErrorCode(It.IsAny<IntPtr>())).Returns(404);
        fixture.Image.Setup(m => m.GetHttpErrorCode(It.IsAny<IntPtr>())).Returns(404);
        await using var engine = fixture.CreateEngine();
        var result = await ConvertAsync(engine, image);
        result.FailureKind.Should().Be(ConversionFailureKind.ConversionError);
        result.HttpErrorCode.Should().Be(404);
        fixture.Pdf.Verify(m => m.Terminate(), Times.Never);
        engine.IsFaulted.Should().BeFalse();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task NativeFailureShouldRecycleBeforeCompletingAsync(bool image)
    {
        var fixture = new NativeRuntimeTestFixture();
        fixture.Pdf.Setup(m => m.Convert(It.IsAny<IntPtr>())).Throws(new InvalidOperationException("native failure"));
        fixture.Image.Setup(m => m.Convert(It.IsAny<IntPtr>())).Throws(new InvalidOperationException("native failure"));
        await using var engine = fixture.CreateEngine();
        var result = await ConvertAsync(engine, image);
        result.FailureKind.Should().Be(ConversionFailureKind.NativeRuntimeError);
        fixture.Pdf.Verify(m => m.Terminate(), Times.Once);
        fixture.Image.Verify(m => m.Terminate(), Times.Once);
        engine.IsFaulted.Should().BeFalse();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task OutputFailureShouldNotRecycleSessionAsync(bool image)
    {
        var fixture = new NativeRuntimeTestFixture();
        fixture.Pdf.Setup(m => m.GetOutput(It.IsAny<IntPtr>(), It.IsAny<Func<int, Stream>>())).Throws(new OutputWriteException(new IOException("output")));
        fixture.Image.Setup(m => m.GetOutput(It.IsAny<IntPtr>(), It.IsAny<Func<int, Stream>>())).Throws(new OutputWriteException(new IOException("output")));
        await using var engine = fixture.CreateEngine();
        var result = await ConvertAsync(engine, image);
        result.FailureKind.Should().Be(ConversionFailureKind.OutputWriteError);
        fixture.Pdf.Verify(m => m.Terminate(), Times.Never);
    }

    [Theory]
    [InlineData(false, "warning")]
    [InlineData(false, "error")]
    [InlineData(false, "finished")]
    [InlineData(false, "phase")]
    [InlineData(false, "progress")]
    [InlineData(true, "warning")]
    [InlineData(true, "error")]
    [InlineData(true, "finished")]
    [InlineData(true, "phase")]
    [InlineData(true, "progress")]
    public async Task CallbackExceptionsShouldNotEscapeNativeCallAsync(bool image, string callback)
    {
        var expected = new InvalidOperationException("subscriber");
        var configuration = new WkHtmlToXConfiguration(platformId: 0, runtimeIdentifier: null)
        {
            WarningAction = _ => throw expected,
            ErrorAction = _ => throw expected,
            FinishedAction = _ => throw expected,
            PhaseChangedAction = _ => throw expected,
            ProgressChangedAction = _ => throw expected,
        };
        var fixture = new NativeRuntimeTestFixture(configuration);
        if (image)
        {
            InvokeCallbackDuringConversion(fixture.Image, callback);
        }
        else
        {
            InvokeCallbackDuringConversion(fixture.Pdf, callback);
        }

        await using var engine = fixture.CreateEngine();
        var result = await ConvertAsync(engine, image);
        result.FailureKind.Should().Be(ConversionFailureKind.CallbackError);
        result.CallbackException.Should().BeSameAs(expected);
        fixture.Pdf.Verify(m => m.Terminate(), Times.Never);
        engine.IsFaulted.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    [InlineData(1024)]
    [InlineData(1025)]
    public async Task WarningsShouldBeBoundedWithoutApplicationCallbackAndResetBetweenRequestsAsync(int length)
    {
        var fixture = new NativeRuntimeTestFixture();
        var message = new string('w', length);
        StringCallback? warning = null;
        fixture.Pdf.Setup(m => m.SetWarningCallback(It.IsAny<IntPtr>(), It.IsAny<StringCallback>()))
            .Callback<IntPtr, StringCallback>((_, callback) => warning = callback);
        fixture.Pdf.Setup(m => m.Convert(It.IsAny<IntPtr>())).Returns(() =>
        {
            warning.Should().NotBeNull();
            for (var i = 0; i < 33; i++)
            {
                warning.Invoke(new IntPtr(2), message);
            }

            return true;
        });
        await using var engine = fixture.CreateEngine();

        var result = await ConvertAsync(engine, image: false);

        result.Success.Should().BeTrue();
        result.Warnings.Should().HaveCount(32);
        var expected = new string('w', Math.Min(length, 1024));
        result.Warnings.Should().OnlyContain(value => value == expected);
        fixture.Pdf.Setup(m => m.Convert(It.IsAny<IntPtr>())).Returns(value: true);
        (await ConvertAsync(engine, image: false)).Warnings.Should().BeEmpty();
        result.Warnings.Should().HaveCount(32);
    }

    [Fact]
    public async Task CallbackReentrancyShouldFailWithoutDeadlockingAsync()
    {
        WkHtmlToXEngine? engine = null;
        var configuration = new WkHtmlToXConfiguration(platformId: 0, runtimeIdentifier: null)
        {
            WarningAction = _ => engine!.Initialize(),
        };
        var fixture = new NativeRuntimeTestFixture(configuration);
        InvokeCallbackDuringConversion(fixture.Pdf, "warning");
        await using (engine = fixture.CreateEngine())
        {
            var result = await ConvertAsync(engine, image: false);
            result.FailureKind.Should().Be(ConversionFailureKind.CallbackError);
            result.CallbackException.Should().BeOfType<InvalidOperationException>();
        }
    }

    private static Task<ConversionResult> ConvertAsync(WkHtmlToXEngine engine, bool image) =>
        image
            ? engine.ConvertImageAsync(NativeRuntimeTestFixture.ImageDocument(), _ => Stream.Null, TestContext.Current.CancellationToken)
            : engine.ConvertPdfAsync(NativeRuntimeTestFixture.PdfDocument(), _ => Stream.Null, TestContext.Current.CancellationToken);

    private static void InvokeCallbackDuringConversion<T>(Mock<T> module, string callback)
        where T : class, IWkHtmlToXModule
    {
        Action? notify = null;
        switch (callback)
        {
            case "warning":
                module.Setup(m => m.SetWarningCallback(It.IsAny<IntPtr>(), It.IsAny<StringCallback>()))
                    .Callback<IntPtr, StringCallback>((pointer, action) => notify = () => action.Invoke(pointer, "warning"));
                break;
            case "error":
                module.Setup(m => m.SetErrorCallback(It.IsAny<IntPtr>(), It.IsAny<StringCallback>()))
                    .Callback<IntPtr, StringCallback>((pointer, action) => notify = () => action.Invoke(pointer, "error"));
                break;
            case "finished":
                module.Setup(m => m.SetFinishedCallback(It.IsAny<IntPtr>(), It.IsAny<IntCallback>()))
                    .Callback<IntPtr, IntCallback>((pointer, action) => notify = () => action.Invoke(pointer, 1));
                break;
            case "phase":
                module.Setup(m => m.SetPhaseChangedCallback(It.IsAny<IntPtr>(), It.IsAny<VoidCallback>()))
                    .Callback<IntPtr, VoidCallback>((pointer, action) => notify = () => action.Invoke(pointer));
                break;
            case "progress":
                module.Setup(m => m.SetProgressChangedCallback(It.IsAny<IntPtr>(), It.IsAny<IntCallback>()))
                    .Callback<IntPtr, IntCallback>((pointer, action) => notify = () => action.Invoke(pointer, 50));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(callback));
        }

        module.Setup(m => m.Convert(It.IsAny<IntPtr>())).Returns(() =>
        {
            notify.Should().NotBeNull();
            notify.Invoke();
            return true;
        });
    }
}
