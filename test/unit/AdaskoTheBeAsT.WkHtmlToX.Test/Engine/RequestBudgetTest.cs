using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.Settings;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Engine;

#pragma warning disable RCS1261 // In-memory streams are synchronously disposed on every supported TFM.
public sealed class RequestBudgetTest
{
    [Fact]
    public void SnapshotShouldReserveWholeRequestBeforeCopyingAnyPayload()
    {
        var document = NativeRuntimeTestFixture.PdfDocument();
        var item = document.ObjectSettings[0];
        item.HtmlContent = null;
        item.HtmlContentByteArray = [65, 0];
        var snapshot = RequestSnapshot.Create(document, 4, bytes =>
        {
            bytes.Should().Be(2);
            item.HtmlContentByteArray[0] = 66;
        });
        snapshot.ObjectSettings[0].HtmlContentByteArray![0].Should().Be(66);
        snapshot.ObjectSettings[0].HtmlContentByteArray.Should().NotBeSameAs(item.HtmlContentByteArray);
    }

    [Theory]
    [InlineData("string")]
    [InlineData("bytes")]
    [InlineData("stream")]
    public async Task SharedInputBudgetShouldCoverAllFormsThroughDeliveryAndRecoverAsync(string form)
    {
        var fixture = new NativeRuntimeTestFixture();
        RequestOwnershipTest.SetOutput(fixture, 4);
        using var input = new RequestTestStream();
        using var output = new RequestTestStream(gateWrites: true);
        var document = NativeRuntimeTestFixture.PdfDocument();
        var item = document.ObjectSettings[0];
        item.HtmlContent = null;
        if (string.Equals(form, "string", StringComparison.Ordinal))
        {
            item.HtmlContent = "\u00e9\u00e9";
            item.Encoding = Encoding.UTF8;
        }
        else if (string.Equals(form, "bytes", StringComparison.Ordinal))
        {
            item.HtmlContentByteArray = new byte[4];
        }
        else
        {
            item.HtmlContentStream = input;
        }

        var options = new WkHtmlToXRequestOptions { MaxInputBytes = 4, MaxBufferedInputBytes = 4 };
        await using var engine = fixture.CreateEngine(requestOptions: options);
        options.MaxBufferedInputBytes = 100;
#pragma warning disable IDISP011 // Output stays borrowed until first is awaited below.
        var first = engine.ConvertPdfAsync(document, _ => output, TestContext.Current.CancellationToken);
#pragma warning restore IDISP011
        try
        {
#pragma warning disable VSTHRD003 // Deterministic output gate holds the first reservation.
            await output.Entered.Task;
#pragma warning restore VSTHRD003
            var second = NativeRuntimeTestFixture.PdfDocument();
            second.ObjectSettings[0].HtmlContent = "x";
            (await engine.ConvertPdfAsync(second, _ => Stream.Null, TestContext.Current.CancellationToken))
                .FailureKind.Should().Be(ConversionFailureKind.ResourceLimit);
            fixture.Pdf.Verify(m => m.Convert(It.IsAny<IntPtr>()), Times.Once);
            (await engine.ConvertImageAsync(NativeRuntimeTestFixture.ImageDocument(), _ => Stream.Null, TestContext.Current.CancellationToken))
                .Success.Should().BeTrue();
            output.Release.TrySetResult(true);
            (await first).Success.Should().BeTrue();
            (await engine.ConvertPdfAsync(second, _ => Stream.Null, TestContext.Current.CancellationToken)).Success.Should().BeTrue();
        }
        finally
        {
            output.Release.TrySetResult(true);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task InputFailureOrCancellationShouldReleaseReservationAsync(bool cancel)
    {
        var fixture = new NativeRuntimeTestFixture();
        using var input = new RequestTestStream(gateReads: true) { FailRead = !cancel };
        using var cancellation = new CancellationTokenSource();
        var document = NativeRuntimeTestFixture.PdfDocument();
        document.ObjectSettings[0].HtmlContent = null;
        document.ObjectSettings[0].HtmlContentStream = input;
        await using var engine = fixture.CreateEngine(requestOptions: new WkHtmlToXRequestOptions { MaxInputBytes = 4, MaxBufferedInputBytes = 4 });
        var first = engine.ConvertPdfAsync(document, _ => Stream.Null, cancellation.Token);
        try
        {
#pragma warning disable VSTHRD003 // Explicit test gate.
            await input.Entered.Task;
#pragma warning restore VSTHRD003
            var second = NativeRuntimeTestFixture.PdfDocument();
            second.ObjectSettings[0].HtmlContent = "test";
            (await engine.ConvertPdfAsync(second, _ => Stream.Null, TestContext.Current.CancellationToken))
                .FailureKind.Should().Be(ConversionFailureKind.ResourceLimit);
            fixture.Loader.Verify(l => l.Load(), Times.Never);
            if (cancel)
            {
#if NET8_0_OR_GREATER
                await cancellation.CancelAsync();
#else
                cancellation.Cancel();
#endif
            }

            input.Release.TrySetResult(true);
            if (cancel)
            {
#pragma warning disable VSTHRD003 // Joins the task after releasing its stream.
                Func<Task> action = async () => await first;
#pragma warning restore VSTHRD003
                await action.Should().ThrowAsync<OperationCanceledException>();
            }
            else
            {
                (await first).FailureKind.Should().Be(ConversionFailureKind.InputReadError);
            }

            (await engine.ConvertPdfAsync(second, _ => Stream.Null, TestContext.Current.CancellationToken)).Success.Should().BeTrue();
        }
        finally
        {
            input.Release.TrySetResult(true);
        }
    }

    [Fact]
    public void SharedInputBudgetMustCoverOneMaximumInput()
    {
        var options = new WkHtmlToXRequestOptions { MaxInputBytes = 5, MaxBufferedInputBytes = 4 };
        Action action = () => options.Snapshot();
        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GrowingStreamMustNotAllocateBeyondItsAdmissionReservationAsync(bool trailingBytes)
    {
        var fixture = new NativeRuntimeTestFixture();
        using var firstInput = new RequestTestStream(gateReads: true);
        using var growing = new MemoryStream();
        growing.SetLength(4);
        var document = NativeRuntimeTestFixture.PdfDocument();
        document.ObjectSettings[0].HtmlContent = null;
        document.ObjectSettings[0].HtmlContentStream = firstInput;
        document.ObjectSettings.Add(new PdfObjectSettings { HtmlContentStream = growing });
        if (trailingBytes)
        {
            document.ObjectSettings.Add(new PdfObjectSettings { HtmlContentByteArray = new byte[4] });
        }

        var limit = trailingBytes ? 12 : 8;
        await using var engine = fixture.CreateEngine(requestOptions: new WkHtmlToXRequestOptions { MaxInputBytes = limit, MaxBufferedInputBytes = limit });
        var conversion = engine.ConvertPdfAsync(document, _ => Stream.Null, TestContext.Current.CancellationToken);
        try
        {
#pragma warning disable VSTHRD003 // Hold preparation before it reaches the second stream.
            await firstInput.Entered.Task;
#pragma warning restore VSTHRD003
            // Deliberately violate borrowed-stream immutability to check defensive allocation limits.
            growing.SetLength(8);
            firstInput.Release.TrySetResult(true);
            (await conversion).FailureKind.Should().Be(ConversionFailureKind.InputReadError);
            growing.Position.Should().Be(0);
            fixture.Loader.Verify(l => l.Load(), Times.Never);
            var next = NativeRuntimeTestFixture.PdfDocument();
            next.ObjectSettings[0].HtmlContent = "12345678";
            (await engine.ConvertPdfAsync(next, _ => Stream.Null, TestContext.Current.CancellationToken)).Success.Should().BeTrue();
        }
        finally
        {
            firstInput.Release.TrySetResult(true);
        }
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void InvalidLaterObjectMustNotReserveOrCloneEarlierPayload(double spacing)
    {
        var document = NativeRuntimeTestFixture.PdfDocument();
        document.ObjectSettings[0].HtmlContent = null;
        document.ObjectSettings[0].HtmlContentByteArray = new byte[4];
        document.ObjectSettings.Add(new PdfObjectSettings { HtmlContent = "x", HeaderSettings = { Spacing = spacing } });
        var reserved = false;
        Action action = () => RequestSnapshot.Create(document, 8, _ => reserved = true);
        action.Should().Throw<ArgumentException>();
        reserved.Should().BeFalse();
    }

    [Theory]
    [InlineData("native-setting")]
    [InlineData("output")]
    public async Task NativeOrDeliveryFailureShouldReleaseInputBudgetAsync(string failure)
    {
        var fixture = new NativeRuntimeTestFixture();
        RequestOwnershipTest.SetOutput(fixture, 4);
        var failSetting = string.Equals(failure, "native-setting", StringComparison.Ordinal);
        fixture.Pdf.Setup(m => m.SetGlobalSetting(It.IsAny<IntPtr>(), "documentTitle", It.IsAny<string?>()))
            .Returns(() => failSetting ? 0 : 1);
        var document = NativeRuntimeTestFixture.PdfDocument();
        document.ObjectSettings[0].HtmlContent = "test";
        document.GlobalSettings.DocumentTitle = "test";
        await using var engine = fixture.CreateEngine(requestOptions: new WkHtmlToXRequestOptions { MaxInputBytes = 4, MaxBufferedInputBytes = 4 });
        var result = await engine.ConvertPdfAsync(
            document,
            _ => throw new IOException("Test delivery failure."),
            TestContext.Current.CancellationToken);
        result.FailureKind.Should().Be(failSetting ? ConversionFailureKind.InvalidInput : ConversionFailureKind.OutputWriteError);
        failSetting = false;
        (await engine.ConvertPdfAsync(document, _ => Stream.Null, TestContext.Current.CancellationToken)).Success.Should().BeTrue();
    }
}
#pragma warning restore RCS1261
