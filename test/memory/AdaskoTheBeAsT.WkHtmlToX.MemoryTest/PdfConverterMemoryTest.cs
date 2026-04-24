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

namespace AdaskoTheBeAsT.WkHtmlToX.MemoryTest;

public sealed class PdfConverterMemoryTest
{
    private const int WarmupConversionCount = 2;
    private const int MeasurementConversionCount = 20;
    private const int EngineLifecycleIterationCount = 8;

    private readonly ITestOutputHelper _output;

    public PdfConverterMemoryTest(
        ITestOutputHelper output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
        DotMemoryUnitTestOutput.SetOutputMethod(_output.WriteLine);
    }

    [DotMemoryUnit(SavingStrategy = SavingStrategy.OnAnyFail, FailIfRunWithoutSupport = false)]
    [Fact]
    public async Task ShouldNotRetainPdfDocumentGraphAfterRepeatedConversionsWithCallbacksAsync()
    {
        var phaseChangedCallCount = 0;
        var progressChangedCallCount = 0;
        var finishedCallCount = 0;

        using var engine = CreateInitializedEngine(new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, runtimeIdentifier: null)
        {
            PhaseChangedAction = _ => phaseChangedCallCount++,
            ProgressChangedAction = _ => progressChangedCallCount++,
            FinishedAction = _ => finishedCallCount++,
        });

        var converter = new PdfConverter(engine);

        await RunPdfConversionsAsync(converter, WarmupConversionCount);
        var memoryCheckPoint = TakeMemoryCheckPoint();

        await RunPdfConversionsAsync(converter, MeasurementConversionCount);

        using (new AssertionScope())
        {
            phaseChangedCallCount.Should().BePositive();
            progressChangedCallCount.Should().BePositive();
            finishedCallCount.Should().Be(WarmupConversionCount + MeasurementConversionCount);
        }

        AssertNoSurvivedPdfDocumentObjects(memoryCheckPoint);
    }

    [DotMemoryUnit(SavingStrategy = SavingStrategy.OnAnyFail, FailIfRunWithoutSupport = false)]
    [Fact]
    public async Task ShouldNotRetainEngineObjectsAfterRepeatedEngineLifecyclesAsync()
    {
        await RunEngineLifecycleAsync(1);
        var memoryCheckPoint = TakeMemoryCheckPoint();

        await RunEngineLifecycleAsync(EngineLifecycleIterationCount);

        AssertNoSurvivedEngineObjects(memoryCheckPoint);
    }

    private static WkHtmlToXEngine CreateInitializedEngine(WkHtmlToXConfiguration configuration)
    {
        var engine = new WkHtmlToXEngine(configuration);
        engine.Initialize();
        return engine;
    }

    private static MemoryCheckPoint? TakeMemoryCheckPoint() =>
        dotMemoryApi.IsEnabled ? dotMemory.Check() : null;

    private static void AssertNoSurvivedPdfDocumentObjects(MemoryCheckPoint? memoryCheckPoint)
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
                    survivedObjects.GetObjects(where => where.Type.Is<HtmlToPdfDocument>())
                        .ObjectsCount.Should().Be(0);
                    survivedObjects.GetObjects(where => where.Type.Is<PdfObjectSettings>())
                        .ObjectsCount.Should().Be(0);
                    survivedObjects.GetObjects(where => where.Type.Is<PdfGlobalSettings>())
                        .ObjectsCount.Should().Be(0);
                }
            });
    }

    private static void AssertNoSurvivedEngineObjects(MemoryCheckPoint? memoryCheckPoint)
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
                    survivedObjects.GetObjects(where => where.Namespace.Like("AdaskoTheBeAsT.WkHtmlToX.Engine"))
                        .ObjectsCount.Should().Be(0);
                    survivedObjects.GetObjects(where => where.Type.Is<PdfConverter>())
                        .ObjectsCount.Should().Be(0);
                }
            });
    }

    private async Task RunPdfConversionsAsync(PdfConverter converter, int count)
    {
        for (var i = 0; i < count; i++)
        {
            await ConvertSingleDocumentAsync(converter).ConfigureAwait(false);
        }
    }

    private async Task RunEngineLifecycleAsync(int count)
    {
        for (var i = 0; i < count; i++)
        {
            using var engine = CreateInitializedEngine(new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, runtimeIdentifier: null));
            var converter = new PdfConverter(engine);

            await ConvertSingleDocumentAsync(converter).ConfigureAwait(false);
        }
    }

    private async Task ConvertSingleDocumentAsync(PdfConverter converter)
    {
        var htmlToPdfGenerator = new HtmlToPdfDocumentGenerator(new SmallHtmlGenerator());
        var document = htmlToPdfGenerator.Generate();
        await using var stream = new MemoryStream();

#pragma warning disable IDISP011
        var converted = await converter.ConvertAsync(
                document,
                _ => stream,
                CancellationToken.None)
            .ConfigureAwait(false);
#pragma warning restore IDISP011

        converted.Should().BeTrue();
        _output.WriteLine($"Converted {stream.Length} bytes.");
    }
}
