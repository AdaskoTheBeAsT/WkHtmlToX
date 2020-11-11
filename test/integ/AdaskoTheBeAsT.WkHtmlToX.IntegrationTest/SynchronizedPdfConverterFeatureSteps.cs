using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AdaskoTheBeAsT.WkHtmlToX.Loaders;
using AdaskoTheBeAsT.WkHtmlToX.Settings;
using Microsoft.IO;
using TechTalk.SpecFlow;

namespace AdaskoTheBeAsT.WkHtmlToX.IntegrationTest
{
    [Binding]
    [Scope(Feature = "SynchronizedPdfConverterFeature")]
    public sealed class SynchronizedPdfConverterFeatureSteps
        : IDisposable
    {
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
        private SynchronizedPdfConverter? _sut;
        private string? _htmlContent;
        private HtmlToPdfDocument? _htmlToPdfDocument;

        public SynchronizedPdfConverterFeatureSteps()
        {
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        }

        [Given("I have SynchronizedPdfConverter")]
        public void GivenIHaveSynchronizedPdfConverter()
        {
            _sut?.Dispose();
            _sut = new SynchronizedPdfConverter();
        }

        [Given("I have sample html to convert '(.*)'")]
        public void GivenIHaveSampleHtmlToConvert(string fileName)
        {
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
#pragma warning disable SEC0116 // Path Tampering Unvalidated File Path
            _htmlContent = File.ReadAllText(Path.Combine("./HtmlSamples", fileName));
#pragma warning restore SEC0116 // Path Tampering Unvalidated File Path
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
        }

        [Given("I created HtmlToPdfDocument")]
        public void GivenICreatedHtmlToPdfDocument()
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
        }

        [When("I convert html to pdf (.*) times")]
        public async Task WhenIConvertHtmlToPdfTimes(int count)
        {
            using var loader = new LibraryLoaderFactory().Create((int)Environment.OSVersion.Platform, runtimeIdentifier: null);
            loader.Load();
            for (int i = 0; i < count; i++)
            {
#pragma warning disable RCS1212 // Remove redundant assignment.
                Stream? stream = null;
                await _sut!.ConvertAsync(
                    _htmlToPdfDocument!,
                    length =>
                    {
                        stream = _recyclableMemoryStreamManager.GetStream(
                            Guid.NewGuid(),
                            "wkhtmltox",
                            length);
                        return stream;
                    },
                    CancellationToken.None).ConfigureAwait(false);
#pragma warning restore RCS1212 // Remove redundant assignment.

#if NETCOREAPP3_1 || NET
                if (stream != null)
                {
                    await stream.DisposeAsync().ConfigureAwait(false);
                }
#else
                stream?.Dispose();
#endif
            }
        }

        [Then("proper pdf should be created")]
#pragma warning disable MA0038 // Make method static
        public void ThenProperPdfShouldBeCreated()
        {
            // noop
        }
#pragma warning restore MA0038 // Make method static

        public void Dispose() => _sut?.Dispose();
    }
}
