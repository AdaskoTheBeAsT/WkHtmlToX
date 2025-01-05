using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.Settings;
using AdaskoTheBeAsT.WkHtmlToX.WorkItems;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test;

public class PdfConverterTest
{
    private readonly Fixture _fixture;
    private readonly Mock<IWkHtmlToXEngine> _engineMock;
    private readonly PdfConverter _sut;

    public PdfConverterTest()
    {
        _fixture = new Fixture();
        _engineMock = new Mock<IWkHtmlToXEngine>(MockBehavior.Strict);
        _sut = new PdfConverter(_engineMock.Object);
    }

    [Fact]
    public async Task ConvertAsyncShouldReturnNullStreamWhenNotConvertedAsync()
    {
        // Arrange
        _engineMock.Setup(e => e.AddConvertWorkItem(It.IsAny<ConvertWorkItemBase>(), It.IsAny<CancellationToken>()))
            .Callback<ConvertWorkItemBase, CancellationToken>((i, _) => i.TaskCompletionSource.SetResult(false));

        var document = new HtmlToPdfDocument();
        var documentTitle = _fixture.Create<string>();
        var captionText = _fixture.Create<string>();
        document.GlobalSettings.DocumentTitle = documentTitle;
        document.ObjectSettings.Add(
            new PdfObjectSettings
            {
                CaptionText = captionText,
                HtmlContent = "<html><head><title>title</title></head><body></body></html>",
            });

        // Act
        var result = await _sut.ConvertAsync(document, _ => Stream.Null, CancellationToken.None);

        // Assert
        using (new AssertionScope())
        {
            _engineMock.Verify(
                e =>
                    e.AddConvertWorkItem(It.IsAny<ConvertWorkItemBase>(), It.IsAny<CancellationToken>()),
                Times.Once);
            result.Should().BeFalse();
        }
    }

    [Fact]
    public async Task ConvertAsyncShouldReturnStreamWhenConvertedAsync()
    {
        // Arrange
        _engineMock.Setup(
                e =>
                    e.AddConvertWorkItem(It.IsAny<ConvertWorkItemBase>(), It.IsAny<CancellationToken>()))
            .Callback<ConvertWorkItemBase, CancellationToken>((i, _) => i.TaskCompletionSource.SetResult(true));

#if NET462_OR_GREATER
        using var memoryStream = new MemoryStream();
#endif
#if NET6_0_OR_GREATER
        await using var memoryStream = new MemoryStream();
#endif

        var document = new HtmlToPdfDocument();
        var documentTitle = _fixture.Create<string>();
        var captionText = _fixture.Create<string>();
        document.GlobalSettings.DocumentTitle = documentTitle;
        document.ObjectSettings.Add(
            new PdfObjectSettings
            {
                CaptionText = captionText,
                HtmlContent = "<html><head><title>title</title></head><body></body></html>",
            });

        // Act
#pragma warning disable IDISP011
        var result = await _sut.ConvertAsync(document, _ => memoryStream, CancellationToken.None);
#pragma warning restore IDISP011

        // Assert
        using (new AssertionScope())
        {
            _engineMock.Verify(
                e =>
                    e.AddConvertWorkItem(It.IsAny<ConvertWorkItemBase>(), It.IsAny<CancellationToken>()),
                Times.Once);
            result.Should().BeTrue();
        }
    }
}
