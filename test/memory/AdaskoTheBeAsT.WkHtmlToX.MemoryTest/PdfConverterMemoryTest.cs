using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using FluentAssertions;
using JetBrains.dotMemoryUnit;
using JetBrains.dotMemoryUnit.Kernel;
using Xunit;
using Xunit.Abstractions;

namespace AdaskoTheBeAsT.WkHtmlToX.MemoryTest;

public sealed class PdfConverterMemoryTest
    : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly WkHtmlToXEngine _engine;

    public PdfConverterMemoryTest(
        ITestOutputHelper output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
        DotMemoryUnitTestOutput.SetOutputMethod(_output.WriteLine);
        _engine = new WkHtmlToXEngine(new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, runtimeIdentifier: null));
        _engine.Initialize();
    }

    public void Dispose() => _engine.Dispose();

    [DotMemoryUnit(SavingStrategy = SavingStrategy.OnAnyFail, FailIfRunWithoutSupport = false)]
    [Fact]
    public async Task ShouldNotLeaveAnyObjectsSurvivedAsync()
    {
        var htmlToPdfGenerator = new HtmlToPdfDocumentGenerator(new SmallHtmlGenerator());
        MemoryCheckPoint? memoryCheckPoint = null;
        if (dotMemoryApi.IsEnabled)
        {
            memoryCheckPoint = dotMemory.Check();
        }

        var doc = htmlToPdfGenerator.Generate();

        if (!Directory.Exists("files"))
        {
            Directory.CreateDirectory("files");
        }

        var converter = new PdfConverter(_engine);
#pragma warning disable SEC0112 // Path Tampering Unvalidated File Path
#pragma warning disable SCS0018 // Potential Path Traversal vulnerability was found where '{0}' in '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.
        await using var stream = new FileStream(
            Path.Combine(
                "Files",
                $"{DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture)}.pdf"),
            FileMode.Create);
#pragma warning restore SCS0018 // Potential Path Traversal vulnerability was found where '{0}' in '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.
#pragma warning restore SEC0112 // Path Tampering Unvalidated File Path
#pragma warning disable IDISP011
        var converted = await converter.ConvertAsync(
            doc,
            _ => stream,
            CancellationToken.None);
#pragma warning restore IDISP011
        _output.WriteLine(converted.ToString(CultureInfo.InvariantCulture));

        if (dotMemoryApi.IsEnabled)
        {
            dotMemory.Check(
                memory =>
                {
                    if (memoryCheckPoint == null)
                    {
                        return;
                    }

                    var objects = memory.GetDifference(memoryCheckPoint.Value)
                        .GetSurvivedObjects()
                        .GetObjects(where => where.Namespace.Like(nameof(AdaskoTheBeAsT)));
                    var objectCount = objects.ObjectsCount;
                    objectCount.Should().BeLessOrEqualTo(5);
                });
        }
    }
}
