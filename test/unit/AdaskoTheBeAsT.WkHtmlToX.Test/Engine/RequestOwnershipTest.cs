using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.Settings;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Engine;

public sealed class RequestOwnershipTest
{
    [Fact]
    public void SnapshotShouldDetachAllMutableBuiltInSettingsAndBuffers()
    {
        var document = NativeRuntimeTestFixture.PdfDocument();
        document.GlobalSettings.PaperSize = new PechkinPaperSize("10mm", "20mm");
        document.GlobalSettings.Margins.Left = 2;
        var item = document.ObjectSettings[0];
        item.HtmlContent = null;
        item.HtmlContentByteArray = [65, 0];
        item.LoadSettings.CustomHeaders = new Dictionary<string, string>(StringComparer.Ordinal) { ["x-test"] = "before" };
        item.HeaderSettings.Left = "before";
        var snapshot = RequestSnapshot.Create(document, maxInputBytes: 10, _ => { });
        item.HtmlContentByteArray[0] = 66;
        item.LoadSettings.CustomHeaders["x-test"] = "after";
        item.HeaderSettings.Left = "after";
        document.GlobalSettings.PaperSize.Width = "30mm";
        document.GlobalSettings.Margins.Left = 9;
        document.ObjectSettings.Clear();

        snapshot.GlobalSettings.PaperSize!.Width.Should().Be("10mm");
        snapshot.GlobalSettings.Margins.Left.Should().Be(2);
        snapshot.ObjectSettings.Should().ContainSingle();
        snapshot.ObjectSettings[0].HtmlContentByteArray![0].Should().Be(65);
        snapshot.ObjectSettings[0].LoadSettings.CustomHeaders!["x-test"].Should().Be("before");
        snapshot.ObjectSettings[0].HeaderSettings.Left.Should().Be("before");
    }

    [Fact]
    public async Task SubmissionShouldSnapshotBeforeReturningToCallerAsync()
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
        var first = engine.ConvertPdfAsync(NativeRuntimeTestFixture.PdfDocument(), _ => Stream.Null, TestContext.Current.CancellationToken);
        try
        {
            entered.Wait(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken).Should().BeTrue();
            var document = NativeRuntimeTestFixture.PdfDocument();
            document.GlobalSettings.DocumentTitle = "before";
            var second = engine.ConvertPdfAsync(document, _ => Stream.Null, TestContext.Current.CancellationToken);
            document.GlobalSettings.DocumentTitle = "after";
            document.ObjectSettings.Clear();
            release.Set();
            (await first).Success.Should().BeTrue();
            (await second).Success.Should().BeTrue();
            fixture.Pdf.Verify(m => m.SetGlobalSetting(It.IsAny<IntPtr>(), "documentTitle", "before"), Times.Once);
            fixture.Pdf.Verify(m => m.SetGlobalSetting(It.IsAny<IntPtr>(), "documentTitle", "after"), Times.Never);
        }
        finally
        {
            release.Set();
        }
    }

    [Theory]
    [InlineData("multiple")]
    [InlineData("none")]
    [InlineData("oversize")]
    [InlineData("output")]
    [InlineData("enum")]
    [InlineData("stdin")]
    public async Task InvalidInputShouldNeverInitializeNativeCodeAsync(string invalid)
    {
        var fixture = new NativeRuntimeTestFixture();
        var document = NativeRuntimeTestFixture.PdfDocument();
        switch (invalid)
        {
            case "multiple":
                document.ObjectSettings[0].HtmlContentByteArray = [65];
                break;
            case "none":
                document.ObjectSettings.Clear();
                break;
            case "output":
                document.GlobalSettings.Out = "not-written.pdf";
                break;
            case "enum":
                document.GlobalSettings.Orientation = (Orientation)999;
                break;
            case "stdin":
                document.ObjectSettings[0].HtmlContent = null;
                document.ObjectSettings[0].Page = "-";
                break;
            default:
                break;
        }

        await using var engine = fixture.CreateEngine(requestOptions: new WkHtmlToXRequestOptions
        {
            MaxInputBytes = string.Equals(invalid, "oversize", StringComparison.Ordinal) ? 1 : 1024,
        });
        var result = await engine.ConvertPdfAsync(document, _ => Stream.Null, TestContext.Current.CancellationToken);
        result.FailureKind.Should().Be(ConversionFailureKind.InvalidInput);
        fixture.Loader.Verify(l => l.Load(), Times.Never);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RejectedNativeSettingShouldNotLeakValuesOrRecycleAsync(bool image)
    {
        var fixture = new NativeRuntimeTestFixture();
        fixture.Pdf.Setup(m => m.SetGlobalSetting(It.IsAny<IntPtr>(), "documentTitle", It.IsAny<string?>())).Returns(0);
        fixture.Image.Setup(m => m.SetGlobalSetting(It.IsAny<IntPtr>(), "in", It.IsAny<string?>())).Returns(0);
        await using var engine = fixture.CreateEngine();
        var pdf = NativeRuntimeTestFixture.PdfDocument();
        pdf.GlobalSettings.DocumentTitle = "private-value";
        var picture = NativeRuntimeTestFixture.ImageDocument();
        picture.ImageSettings.In = "private-value";
        var result = image
            ? await engine.ConvertImageAsync(picture, _ => Stream.Null, TestContext.Current.CancellationToken)
            : await engine.ConvertPdfAsync(pdf, _ => Stream.Null, TestContext.Current.CancellationToken);
        result.FailureKind.Should().Be(ConversionFailureKind.InvalidInput);
        result.Exception!.Message.Should().NotContain("private-value");
        fixture.Pdf.Verify(m => m.Terminate(), Times.Never);
        fixture.Pdf.Verify(m => m.Convert(It.IsAny<IntPtr>()), Times.Never);
        fixture.Image.Verify(m => m.Convert(It.IsAny<IntPtr>()), Times.Never);
    }

    [Fact]
    public async Task SlowDestinationShouldNotOccupyNativeThreadAndShouldHoldAdmissionAsync()
    {
        var fixture = new NativeRuntimeTestFixture();
        SetOutput(fixture, length: 4);
        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        var nativeThread = 0;
        fixture.Pdf.Setup(m => m.Convert(It.IsAny<IntPtr>())).Returns(() =>
        {
            nativeThread = Environment.CurrentManagedThreadId;
            return true;
        });
        await using var engine = fixture.CreateEngine(requestOptions: new WkHtmlToXRequestOptions { MaxConcurrentRequests = 2 });
        var first = engine.ConvertPdfAsync(
            NativeRuntimeTestFixture.PdfDocument(),
            _ =>
            {
                Environment.CurrentManagedThreadId.Should().NotBe(nativeThread);
                fixture.Pdf.Verify(m => m.DestroyConverter(It.IsAny<IntPtr>()), Times.Once);
                entered.Set();
                release.Wait(TestContext.Current.CancellationToken);
                return Stream.Null;
            },
            TestContext.Current.CancellationToken);
        try
        {
            entered.Wait(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken).Should().BeTrue();
            (await engine.ConvertImageAsync(NativeRuntimeTestFixture.ImageDocument(), _ => Stream.Null, TestContext.Current.CancellationToken))
                .Success.Should().BeTrue();
            first.IsCompleted.Should().BeFalse();
            release.Set();
            (await first).Success.Should().BeTrue();
        }
        finally
        {
            release.Set();
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task OutputLimitsShouldRejectAndReleaseBudgetAsync(bool sharedBudget)
    {
        var fixture = new NativeRuntimeTestFixture();
        SetOutput(fixture, length: 4);
        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        await using var engine = fixture.CreateEngine(requestOptions: new WkHtmlToXRequestOptions
        {
            MaxOutputBytes = sharedBudget ? 4 : 3,
            MaxBufferedOutputBytes = 4,
        });
        var first = engine.ConvertPdfAsync(
            NativeRuntimeTestFixture.PdfDocument(),
            _ =>
            {
                entered.Set();
                release.Wait(TestContext.Current.CancellationToken);
                return Stream.Null;
            },
            TestContext.Current.CancellationToken);
        try
        {
            if (sharedBudget)
            {
                entered.Wait(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken).Should().BeTrue();
                (await engine.ConvertPdfAsync(NativeRuntimeTestFixture.PdfDocument(), _ => Stream.Null, TestContext.Current.CancellationToken))
                    .FailureKind.Should().Be(ConversionFailureKind.ResourceLimit);
                release.Set();
                (await first).Success.Should().BeTrue();
                (await engine.ConvertPdfAsync(NativeRuntimeTestFixture.PdfDocument(), _ => Stream.Null, TestContext.Current.CancellationToken))
                    .Success.Should().BeTrue();
            }
            else
            {
                (await first).FailureKind.Should().Be(ConversionFailureKind.ResourceLimit);
                entered.IsSet.Should().BeFalse();
            }

            fixture.Pdf.Verify(m => m.Terminate(), Times.Never);
        }
        finally
        {
            release.Set();
        }
    }

    [Fact]
    public async Task AdmissionShouldRemainBoundedDuringDeliveryAsync()
    {
        var fixture = new NativeRuntimeTestFixture();
        SetOutput(fixture, length: 4);
        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        var options = new WkHtmlToXRequestOptions { MaxConcurrentRequests = 1 };
        await using var engine = fixture.CreateEngine(requestOptions: options);
        options.MaxConcurrentRequests = 100;
        var first = engine.ConvertPdfAsync(
            NativeRuntimeTestFixture.PdfDocument(),
            _ =>
            {
                entered.Set();
                release.Wait(TestContext.Current.CancellationToken);
                return Stream.Null;
            },
            TestContext.Current.CancellationToken);
        try
        {
            entered.Wait(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken).Should().BeTrue();
            (await engine.ConvertImageAsync(NativeRuntimeTestFixture.ImageDocument(), _ => Stream.Null, TestContext.Current.CancellationToken))
                .FailureKind.Should().Be(ConversionFailureKind.Overloaded);
            release.Set();
            (await first).Success.Should().BeTrue();
            (await engine.ConvertImageAsync(NativeRuntimeTestFixture.ImageDocument(), _ => Stream.Null, TestContext.Current.CancellationToken))
                .Success.Should().BeTrue();
        }
        finally
        {
            release.Set();
        }
    }

    internal static void SetOutput(NativeRuntimeTestFixture fixture, int length) =>
        fixture.Pdf.Setup(m => m.GetOutput(It.IsAny<IntPtr>(), It.IsAny<Func<int, Stream>>()))
            .Callback<IntPtr, Func<int, Stream>>((_, create) =>
            {
                try
                {
#pragma warning disable IDISP004 // The engine owns the intermediate buffer.
                    create.Invoke(length);
#pragma warning restore IDISP004
                }
                catch (Exception exception)
                {
                    throw new OutputWriteException(exception);
                }
            });
}
