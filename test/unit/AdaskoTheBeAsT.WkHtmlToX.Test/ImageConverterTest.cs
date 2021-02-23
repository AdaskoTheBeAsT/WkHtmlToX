using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.WorkItems;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test
{
    public class ImageConverterTest
    {
        private readonly Mock<IWkHtmlToXEngine> _engineMock;
        private readonly ImageConverter _sut;

        public ImageConverterTest()
        {
            _engineMock = new Mock<IWkHtmlToXEngine>();
            _sut = new ImageConverter(_engineMock.Object);
        }

        [Fact]
        public async Task ConvertAsyncShouldReturnNullStreamWhenNotConverted()
        {
            // Arrange
            _engineMock.Setup(e => e.AddConvertWorkItem(It.IsAny<ConvertWorkItemBase>(), It.IsAny<CancellationToken>()))
                .Callback<ConvertWorkItemBase, CancellationToken>(
                    (
                        i,
                        _) =>
                    {
                        i.TaskCompletionSource.SetResult(false);
                    });

            var document = new HtmlToImageDocument();

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
        public async Task ConvertAsyncShouldReturnStreamWhenConverted()
        {
            // Arrange
            _engineMock.Setup(
                    e =>
                        e.AddConvertWorkItem(It.IsAny<ConvertWorkItemBase>(), It.IsAny<CancellationToken>()))
                .Callback<ConvertWorkItemBase, CancellationToken>(
                    (
                        i,
                        _) =>
                    {
                        i.TaskCompletionSource.SetResult(true);
                    });
            using var memoryStream = new MemoryStream();

            var document = new HtmlToImageDocument();

            // Act
            var result = await _sut.ConvertAsync(document, _ => memoryStream, CancellationToken.None);

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
}
