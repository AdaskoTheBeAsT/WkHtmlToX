using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AdaskoTheBeAsT.WkHtmlToX.Settings;
using Microsoft.IO;
using TechTalk.SpecFlow;

namespace AdaskoTheBeAsT.WkHtmlToX.IntegrationTest.Steps;

[Binding]
[Scope(Feature = "PdfConverterFeature")]
public sealed class PdfConverterFeatureSteps
{
    private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
    private PdfConverter? _sut;
    private string? _htmlContent;
    private HtmlToPdfDocument? _htmlToPdfDocument;

    public PdfConverterFeatureSteps()
    {
        _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
    }

    [Given("I have SynchronizedPdfConverter")]
    public void GivenIHaveSynchronizedPdfConverter()
    {
        _sut = new PdfConverter(GlobalInitializer.Engine!);
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
    public async Task WhenIConvertHtmlToPdfTimesAsync(int count)
    {
        for (var i = 0; i < count; i++)
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

#if NET6_0_OR_GREATER
#pragma warning disable S2583 // Conditionally executed code should be reachable
            if (stream != null)
            {
                await stream.DisposeAsync().ConfigureAwait(false);
            }
#pragma warning restore S2583 // Conditionally executed code should be reachable
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
}
