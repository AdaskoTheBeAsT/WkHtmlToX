using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AdaskoTheBeAsT.WkHtmlToX.Settings;
using FluentAssertions;
using Microsoft.IO;
using TechTalk.SpecFlow;

namespace AdaskoTheBeAsT.WkHtmlToX.IntegrationTest
{
    [Binding]
    [Scope(Feature = "MultipleConversionFeature")]
    public sealed class MultipleConversionFeatureSteps
    {
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
        private PdfConverter? _sut;
        private string? _htmlContent;
        private HtmlToPdfDocument? _htmlToPdfDocument;
        private byte[]? _content1;
        private byte[]? _content2;

        public MultipleConversionFeatureSteps()
        {
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        }

        [Given(@"I have SynchronizedPdfConverter")]
        public void GivenIHaveSynchronizedPdfConverter()
        {
            _sut = new PdfConverter(GlobalInitializer.Engine!);
        }

        [Given(@"I have complex html")]
        public void GivenIHaveComplexHtml()
        {
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
#pragma warning disable SEC0116 // Path Tampering Unvalidated File Path
            _htmlContent = File.ReadAllText("./HtmlSamples/Bug0002SameHtmlTwice.html");
#pragma warning restore SEC0116 // Path Tampering Unvalidated File Path
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
        }

        [When(@"I convert first time")]
        public async Task WhenIConvertFirstTimeAsync()
        {
            _content1 = await GenerateContentAsync().ConfigureAwait(false);
        }

        [When(@"I convert same html second time")]
        public async Task WhenIConvertSameHtmlSecondTimeAsync()
        {
            _content2 = await GenerateContentAsync().ConfigureAwait(false);
        }

        [Then(@"I should obtain files with same length")]
        public void ThenIShouldObtainFilesWithSameLength()
        {
            _content1.Should().HaveCount(_content2?.Length ?? 0);
        }

        private async Task<byte[]> GenerateContentAsync()
        {
            _htmlToPdfDocument = new HtmlToPdfDocument
            {
                GlobalSettings = new PdfGlobalSettings
                {
                    DocumentTitle = "Sample",
                },
                ObjectSettings =
                {
                    new PdfObjectSettings
                    {
                        HtmlContent = _htmlContent,
                    },
                },
            };

            Stream? stream = null;
#pragma warning disable IDISP001 // Dispose created.
            var ms = new MemoryStream();
#pragma warning restore IDISP001 // Dispose created.

            try
            {
                await _sut!.ConvertAsync(
                        _htmlToPdfDocument!,
                        length =>
                        {
                            stream?.Dispose();
                            stream = _recyclableMemoryStreamManager.GetStream(
                                Guid.NewGuid(),
                                "wkhtmltox",
                                length);
                            return stream;
                        },
                        CancellationToken.None)
                    .ConfigureAwait(false);

                stream!.Position = 0;
                await stream.CopyToAsync(ms).ConfigureAwait(false);
                return ms.ToArray();
            }
            finally
            {
#if NETCOREAPP3_1 || NET
                if (stream != null)
                {
                    await stream.DisposeAsync().ConfigureAwait(false);
                }

                await ms.DisposeAsync().ConfigureAwait(false);
#else
                stream?.Dispose();
                ms?.Dispose();
#endif
            }
        }
    }
}
