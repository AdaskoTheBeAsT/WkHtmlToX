using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Documents;
using Microsoft.IO;
using TechTalk.SpecFlow;

namespace AdaskoTheBeAsT.WkHtmlToX.IntegrationTest.Steps;

[Binding]
[Scope(Feature = "ImageConverterFeature")]
public class ImageConverterFeatureStepDefinitions
{
    private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
    private ImageConverter? _sut;
    private string? _filePath;
    private string? _outputFilePath;
    private HtmlToImageDocument? _htmlToImageDocument;

    public ImageConverterFeatureStepDefinitions()
    {
        _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
    }

    [Given("I have SynchronizedImageConverter")]
    public void GivenIHaveSynchronizedImageConverter()
    {
        _sut = new ImageConverter(GlobalInitializer.Engine!);
    }

    [Given("I have sample html to convert '([^']*)'")]
    public void GivenIHaveSampleHtmlToConvert(string fileName)
    {
        _filePath = Path.Combine("./HtmlSamples", fileName);
        _outputFilePath = Path.Combine("./HtmlSamples", $"{Guid.NewGuid()}.png");
    }

    [Given("I created HtmlToImageDocument")]
    public void GivenICreatedHtmlToImageDocument()
    {
        _htmlToImageDocument = new HtmlToImageDocument
        {
            ImageSettings =
            {
                Format = "png",
                Quality = "94",
                In = _filePath,
                Out = _outputFilePath,
            },
        };
    }

    [When("I convert html to image (.*) times")]
    public async Task WhenIConvertHtmlToImageTimesAsync(int count)
    {
        for (var i = 0; i < count; i++)
        {
#pragma warning disable RCS1212 // Remove redundant assignment.
            Stream? stream = null;
            await _sut!.ConvertAsync(
                    _htmlToImageDocument!,
                    length =>
                    {
                        stream = _recyclableMemoryStreamManager.GetStream(
                            Guid.NewGuid(),
                            "wkhtmltox",
                            length);
                        return stream;
                    },
                    CancellationToken.None)
                .ConfigureAwait(false);
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

    [Then("proper image should be created")]
#pragma warning disable MA0038 // Make method static
    public void ThenProperImageShouldBeCreated()
    {
        // noop
    }
#pragma warning restore MA0038 // Make method static
}
