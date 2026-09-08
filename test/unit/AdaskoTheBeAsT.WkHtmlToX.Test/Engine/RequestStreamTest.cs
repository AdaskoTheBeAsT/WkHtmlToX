using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Engine;

#pragma warning disable RCS1261 // These in-memory streams use synchronous disposal on every supported TFM.
public sealed class RequestStreamTest
{
    [Fact]
    public async Task SlowInputShouldNotOccupyNativeWorkerOrTransferOwnershipAsync()
    {
        var fixture = new NativeRuntimeTestFixture();
        using var stream = new GatedInputStream();
        await using var engine = fixture.CreateEngine();
        var document = NativeRuntimeTestFixture.PdfDocument();
        document.ObjectSettings[0].HtmlContent = null;
        document.ObjectSettings[0].HtmlContentStream = stream;
        var first = engine.ConvertPdfAsync(document, _ => Stream.Null, TestContext.Current.CancellationToken);
        try
        {
#pragma warning disable VSTHRD003 // Explicit regression-test gate.
            await stream.Entered.Task;
#pragma warning restore VSTHRD003
            (await engine.ConvertImageAsync(NativeRuntimeTestFixture.ImageDocument(), _ => Stream.Null, TestContext.Current.CancellationToken))
                .Success.Should().BeTrue();
            first.IsCompleted.Should().BeFalse();
            fixture.Pdf.Verify(m => m.Convert(It.IsAny<IntPtr>()), Times.Never);
            stream.Release.TrySetResult(true);
            (await first).Success.Should().BeTrue();
            stream.CanRead.Should().BeTrue();
            stream.Position.Should().Be(stream.Length);
        }
        finally
        {
            stream.Release.TrySetResult(true);
        }
    }

    [Fact]
    public async Task CanceledInputShouldFinishBeforeReleasingBorrowedStreamAsync()
    {
        var fixture = new NativeRuntimeTestFixture();
        using var stream = new GatedInputStream();
        using var cancellation = new CancellationTokenSource();
        await using var engine = fixture.CreateEngine();
        var document = NativeRuntimeTestFixture.PdfDocument();
        document.ObjectSettings[0].HtmlContent = null;
        document.ObjectSettings[0].HtmlContentStream = stream;
        var conversion = engine.ConvertPdfAsync(document, _ => Stream.Null, cancellation.Token);
        try
        {
#pragma warning disable VSTHRD003 // Explicit regression-test gate.
            await stream.Entered.Task;
#pragma warning restore VSTHRD003
#if NET8_0_OR_GREATER
            await cancellation.CancelAsync();
#else
            cancellation.Cancel();
#endif
            conversion.IsCompleted.Should().BeFalse();
            stream.Release.TrySetResult(true);
#pragma warning disable VSTHRD003 // Joins the conversion after releasing the deterministic read gate.
            Func<Task<ConversionResult>> action = async () => await conversion;
#pragma warning restore VSTHRD003
            await action.Should().ThrowAsync<OperationCanceledException>();
            stream.CanRead.Should().BeTrue();
            fixture.Loader.Verify(l => l.Load(), Times.Never);
        }
        finally
        {
            stream.Release.TrySetResult(true);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UnsupportedStreamShouldBeRejectedBeforeNativeWorkAsync(bool unreadable)
    {
        var fixture = new NativeRuntimeTestFixture();
        using var stream = new UnsupportedInputStream(unreadable);
        await using var engine = fixture.CreateEngine();
        var document = NativeRuntimeTestFixture.PdfDocument();
        document.ObjectSettings[0].HtmlContent = null;
        document.ObjectSettings[0].HtmlContentStream = stream;
        var result = await engine.ConvertPdfAsync(document, _ => Stream.Null, TestContext.Current.CancellationToken);
        result.FailureKind.Should().Be(ConversionFailureKind.InvalidInput);
        fixture.Loader.Verify(l => l.Load(), Times.Never);
    }

    private sealed class UnsupportedInputStream(bool unreadable) : MemoryStream
    {
        public override bool CanRead => !unreadable;

        public override bool CanSeek => unreadable;
    }

    private sealed class GatedInputStream() : MemoryStream(new byte[4])
    {
        internal TaskCompletionSource<bool> Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        internal TaskCompletionSource<bool> Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            Entered.TrySetResult(true);
#pragma warning disable VSTHRD003 // Models a stream that does not observe cancellation until its read returns.
            await Release.Task;
#pragma warning restore VSTHRD003
            cancellationToken.ThrowIfCancellationRequested();
            return await base.ReadAsync(buffer, offset, Math.Min(count, 1), cancellationToken);
        }
    }
}
#pragma warning restore RCS1261
