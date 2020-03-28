using System;
using System.IO;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AdaskoTheBeAsT.WkHtmlToX.Loaders;
using AdaskoTheBeAsT.WkHtmlToX.Settings;
using TechTalk.SpecFlow;

namespace AdaskoTheBeAsT.WkHtmlToX.IntegrationTest
{
    [Binding]
    [Scope(Feature = "SynchronizedPdfConverterFeature")]
    public sealed class SynchronizedPdfConverterFeatureSteps
        : IDisposable
    {
        private SynchronizedPdfConverter? _sut;
        private string? _htmlContent;
        private HtmlToPdfDocument? _htmlToPdfDocument;

        [Given(@"I have SynchronizedPdfConverter")]
        public void GivenIHaveSynchronizedPdfConverter()
        {
            _sut = new SynchronizedPdfConverter();
        }

        [Given(@"I have sample html to convert '(.*)'")]
        public void GivenIHaveSampleHtmlToConvert(string fileName)
        {
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
            _htmlContent = File.ReadAllText(Path.Combine("./HtmlSamples", fileName));
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
        }

        [Given(@"I created HtmlToPdfDocument")]
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

        [When(@"I convert html to pdf (.*) times")]
        public async Task WhenIConvertHtmlToPdfTimes(int count)
        {
            using var loader = new LibraryLoaderFactory().Create((int)Environment.OSVersion.Platform, null);
            loader.Load();
            for (int i = 0; i < count; i++)
            {
                var stream = await _sut!.ConvertAsync(_htmlToPdfDocument!);
#pragma warning disable AsyncFixer02 // Long running or blocking operations under an async method
                stream.Dispose();
#pragma warning restore AsyncFixer02 // Long running or blocking operations under an async method
            }
        }

        [Then(@"proper pdf should be created")]
        public void ThenProperPdfShouldBeCreated()
        {
            // noop
        }

        public void Dispose() => _sut?.Dispose();
    }
}
