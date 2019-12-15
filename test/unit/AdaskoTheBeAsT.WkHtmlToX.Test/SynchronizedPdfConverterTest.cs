using System;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AdaskoTheBeAsT.WkHtmlToX.Loaders;
using AdaskoTheBeAsT.WkHtmlToX.Settings;
using FluentAssertions;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test
{
    public sealed class SynchronizedPdfConverterTest
        : IDisposable
    {
        private readonly SynchronizedPdfConverter _sut;

        public SynchronizedPdfConverterTest()
        {
            _sut = new SynchronizedPdfConverter();
        }

        public void Dispose() => _sut?.Dispose();

        [Fact]
        public void ConvertAsyncShouldWhen()
        {
            // Arrange
            var document = new HtmlToPdfDocument
            {
                GlobalSettings = new PdfGlobalSettings
                {
                    DocumentTitle = "Sample",
                },
                ObjectSettings =
                {
                    new PdfObjectSettings
                    {
                        HtmlContent = @"
<!DOCTYPE html>
<html>
<body>
<h1 style=""color:blue;"">This is a Blue Heading</h1>
</body>
</html>",
                    },
                },
            };

            Func<Task> action = async () =>
            {
                using (var loader = new LibraryLoaderFactory().Create(null))
                {
                    loader.Load();
                    for (int i = 0; i < 100; i++)
                    {
                        var stream = await _sut.ConvertAsync(
                            document);
                        stream.Dispose();
                    }
                }
            };

            // Act & Assert
            action.Should().NotThrow();
        }
    }
}
