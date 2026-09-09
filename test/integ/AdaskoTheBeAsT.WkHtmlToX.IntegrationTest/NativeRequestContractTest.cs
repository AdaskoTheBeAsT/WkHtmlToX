using System.IO;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.Settings;
using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.IntegrationTest;

public sealed class NativeRequestContractTest
{
    [Fact]
    public async Task PageInputShouldUseNativeUrlInsteadOfInlineHtmlAsync()
    {
        await using var engine = new WkHtmlToXEngine(new WkHtmlToXConfiguration());
#pragma warning disable RCS1261 // In-memory output is synchronously disposable on all targets.
        using var output = new MemoryStream();
#pragma warning restore RCS1261
#pragma warning disable IDISP011 // Caller retains the borrowed output until conversion completes.
        var result = await engine.ConvertPdfAsync(
            new HtmlToPdfDocument { ObjectSettings = { new PdfObjectSettings { Page = "about:blank" } } },
            _ => output,
            TestContext.Current.CancellationToken);
#pragma warning restore IDISP011
        result.Success.Should().BeTrue();
        output.Length.Should().BePositive();
    }

    [Fact]
    public async Task NativeSettingRejectionShouldLeaveEngineUsableAsync()
    {
        await using var engine = new WkHtmlToXEngine(new WkHtmlToXConfiguration());
        var document = new HtmlToImageDocument
        {
            ImageSettings = new ImageSettings { In = "about:blank", Format = "png", Quality = "not-a-number" },
        };
        var rejected = await engine.ConvertImageAsync(document, _ => Stream.Null, TestContext.Current.CancellationToken);
        rejected.FailureKind.Should().Be(ConversionFailureKind.InvalidInput);
        rejected.Exception!.Message.Should().Contain("quality").And.NotContain("not-a-number");
        document.ImageSettings.Quality = "94";
        (await engine.ConvertImageAsync(document, _ => Stream.Null, TestContext.Current.CancellationToken))
            .Success.Should().BeTrue();
        engine.IsFaulted.Should().BeFalse();
    }

    [Fact]
    public async Task OutputLimitShouldApplyBeforeDestinationCreationAsync()
    {
        await using var engine = new WkHtmlToXEngine(new WkHtmlToXConfiguration
        {
            RequestOptions = new WkHtmlToXRequestOptions { MaxOutputBytes = 1 },
        });
        var destinationCreated = false;
        var result = await engine.ConvertPdfAsync(
            new HtmlToPdfDocument { ObjectSettings = { new PdfObjectSettings { HtmlContent = "<p>output limit</p>" } } },
            _ =>
            {
                destinationCreated = true;
                return Stream.Null;
            },
            TestContext.Current.CancellationToken);
        result.FailureKind.Should().Be(ConversionFailureKind.ResourceLimit);
        destinationCreated.Should().BeFalse();
        engine.IsFaulted.Should().BeFalse();
    }
}
