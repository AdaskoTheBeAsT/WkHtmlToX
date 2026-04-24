using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.Settings;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using JetBrains.dotMemoryUnit;
using JetBrains.dotMemoryUnit.Kernel;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.RuntimeMemoryTest;

public sealed class ImageConverterMemoryTest
{
    private const int WarmupConversionCount = 2;
    private const int MeasurementConversionCount = 20;
    private const string HtmlContent = @"<html><body><p>This paragraph contains enough content for repeatable image conversion memory testing.</p><p>Additional text keeps the rendering path stable.</p></body></html>";

    private readonly ITestOutputHelper _output;

    public ImageConverterMemoryTest(ITestOutputHelper output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
        DotMemoryUnitTestOutput.SetOutputMethod(_output.WriteLine);
    }

    [DotMemoryUnit(SavingStrategy = SavingStrategy.OnAnyFail, FailIfRunWithoutSupport = false)]
    [Fact]
    public async Task ShouldNotRetainImageDocumentGraphAfterRepeatedConversionsWithCallbacksAsync()
    {
        var phaseChangedCallCount = 0;
        var progressChangedCallCount = 0;
        var finishedCallCount = 0;
        var htmlFilePath = CreateTemporaryHtmlFile();

        try
        {
            using var engine = CreateInitializedEngine(new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, runtimeIdentifier: null)
            {
                PhaseChangedAction = _ => phaseChangedCallCount++,
                ProgressChangedAction = _ => progressChangedCallCount++,
                FinishedAction = _ => finishedCallCount++,
            });

            var converter = new ImageConverter(engine);

            await RunImageConversionsAsync(converter, htmlFilePath);
            var memoryCheckPoint = TakeMemoryCheckPoint();

            await RunImageConversionsAsync(converter, htmlFilePath, MeasurementConversionCount);

            using (new AssertionScope())
            {
                phaseChangedCallCount.Should().BeGreaterThan(0);
                progressChangedCallCount.Should().BeGreaterThan(0);
                finishedCallCount.Should().Be(WarmupConversionCount + MeasurementConversionCount);
            }

            AssertNoSurvivedImageDocumentObjects(memoryCheckPoint);
        }
        finally
        {
            DeleteTemporaryFile(htmlFilePath);
        }
    }

    private static WkHtmlToXEngine CreateInitializedEngine(WkHtmlToXConfiguration configuration)
    {
        var engine = new WkHtmlToXEngine(configuration);
        engine.Initialize();
        return engine;
    }

    private static MemoryCheckPoint? TakeMemoryCheckPoint() =>
        dotMemoryApi.IsEnabled ? dotMemory.Check() : null;

    private static void AssertNoSurvivedImageDocumentObjects(MemoryCheckPoint? memoryCheckPoint)
    {
        if (!dotMemoryApi.IsEnabled || memoryCheckPoint is null)
        {
            return;
        }

        dotMemory.Check(
            memory =>
            {
                var survivedObjects = memory.GetDifference(memoryCheckPoint.Value)
                    .GetSurvivedObjects();

                using (new AssertionScope())
                {
                    survivedObjects.GetObjects(where => where.Type.Is<HtmlToImageDocument>())
                        .ObjectsCount.Should().Be(0);
                    survivedObjects.GetObjects(where => where.Type.Is<ImageSettings>())
                        .ObjectsCount.Should().Be(0);
                    survivedObjects.GetObjects(where => where.Type.Is<LoadSettings>())
                        .ObjectsCount.Should().Be(0);
                    survivedObjects.GetObjects(where => where.Type.Is<WebSettings>())
                        .ObjectsCount.Should().Be(0);
                }
            });
    }

    private static string CreateTemporaryHtmlFile()
    {
        var htmlFilePath = Path.Combine(Path.GetTempPath(), $"wkhtmltox-image-memory-{Guid.NewGuid():N}.html");
#pragma warning disable SEC0116
#pragma warning disable SCS0018
        File.WriteAllText(htmlFilePath, HtmlContent);
#pragma warning restore SCS0018
#pragma warning restore SEC0116
        return htmlFilePath;
    }

    private static void DeleteTemporaryFile(string filePath)
    {
        if (File.Exists(filePath))
        {
#pragma warning disable SEC0116
#pragma warning disable SCS0018
            File.Delete(filePath);
#pragma warning restore SCS0018
#pragma warning restore SEC0116
        }
    }

    private async Task RunImageConversionsAsync(ImageConverter converter, string htmlFilePath, int count = WarmupConversionCount)
    {
        for (var i = 0; i < count; i++)
        {
            await ConvertSingleDocumentAsync(converter, htmlFilePath).ConfigureAwait(false);
        }
    }

    private async Task ConvertSingleDocumentAsync(ImageConverter converter, string htmlFilePath)
    {
        var document = new HtmlToImageDocument
        {
            ImageSettings =
            {
                Format = "png",
                In = htmlFilePath,
                Out = string.Empty,
                Quality = "94",
            },
        };

        await using var stream = new MemoryStream();

#pragma warning disable IDISP011
        var converted = await converter.ConvertAsync(
                document,
                _ => stream,
                CancellationToken.None)
            .ConfigureAwait(false);
#pragma warning restore IDISP011

        using (new AssertionScope())
        {
            converted.Should().BeTrue();
            stream.Length.Should().BeGreaterThan(0);
        }

        _output.WriteLine($"Converted {stream.Length} image bytes.");
    }
}
