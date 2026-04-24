using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.Settings;
using AwesomeAssertions;
using Microsoft.IO;
using Reqnroll;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.IntegrationTest.Steps;

[Binding]
[Scope(Feature = "MultipleConversion")]
public sealed class MultipleConversionSteps
    : IDisposable
{
    private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
    private PdfConverter? _sut;
    private WkHtmlToXEngine? _ownedEngine;
    private string? _htmlContent;
    private HtmlToPdfDocument? _htmlToPdfDocument;
    private byte[]? _content1;
    private byte[]? _content2;

    public MultipleConversionSteps()
    {
        _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
    }

    [Given("I have SynchronizedPdfConverter")]
    public void GivenIHaveSynchronizedPdfConverter()
    {
        DisposeOwnedEngine();
#pragma warning disable IDISP003 // Dispose previous before re-assigning.
        _ownedEngine = new WkHtmlToXEngine(new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, runtimeIdentifier: null));
#pragma warning restore IDISP003 // Dispose previous before re-assigning.
        _ownedEngine.Initialize();
        _sut = new PdfConverter(_ownedEngine);
    }

    [Given("I have complex html")]
    public void GivenIHaveComplexHtml()
    {
#pragma warning disable SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
#pragma warning disable SEC0116 // Path Tampering Unvalidated File Path
        _htmlContent = File.ReadAllText("./HtmlSamples/Bug0002SameHtmlTwice.html");
#pragma warning restore SEC0116 // Path Tampering Unvalidated File Path
#pragma warning restore SCS0018 // Path traversal: injection possible in {1} argument passed to '{0}'
    }

    [When("I convert first time")]
    public async Task WhenIConvertFirstTimeAsync()
    {
        _content1 = await GenerateContentAsync().ConfigureAwait(false);
    }

    [When("I convert same html second time")]
    public async Task WhenIConvertSameHtmlSecondTimeAsync()
    {
        _content2 = await GenerateContentAsync().ConfigureAwait(false);
    }

    [Then("I should obtain files with same length")]
    public void ThenIShouldObtainFilesWithSameLength()
    {
        _content1.Should().HaveCount(_content2?.Length ?? 0);
    }

    [AfterScenario]
    public void AfterScenario()
    {
        DisposeOwnedEngine();
    }

    public void Dispose()
    {
        DisposeOwnedEngine();
    }

    private void DisposeOwnedEngine()
    {
        _ownedEngine?.Dispose();
        _ownedEngine = null;
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
                        // ReSharper disable once AccessToDisposedClosure
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
#if NET8_0_OR_GREATER
            await stream.CopyToAsync(ms, TestContext.Current.CancellationToken).ConfigureAwait(false);
#else
            await stream.CopyToAsync(ms).ConfigureAwait(false);
#endif
            return ms.ToArray();
        }
        finally
        {
#if NET8_0_OR_GREATER
            if (stream != null)
            {
                await stream.DisposeAsync().ConfigureAwait(false);
            }

            await ms.DisposeAsync().ConfigureAwait(false);
#else
            stream?.Dispose();
            ms.Dispose();
#endif
        }
    }
}
