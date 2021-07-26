using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using FluentAssertions;
using JetBrains.dotMemoryUnit;
using Xunit;
using Xunit.Abstractions;

namespace AdaskoTheBeAsT.WkHtmlToX.MemoryTest
{
    public sealed class PdfConverterMemoryTest
        : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly IWkHtmlToXEngine _engine;

        public PdfConverterMemoryTest(
            ITestOutputHelper output)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
            DotMemoryUnitTestOutput.SetOutputMethod(_output.WriteLine);
            _engine = new WkHtmlToXEngine(new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, null));
            _engine.Initialize();
        }

        public void Dispose() => _engine.Dispose();

        [DotMemoryUnit(SavingStrategy = SavingStrategy.OnAnyFail)]
        [Fact]
        public async Task ShouldNotLeaveAnyObjectsSurvivedAsync()
        {
            var htmlToPdfGenerator = new HtmlToPdfDocumentGenerator(new SmallHtmlGenerator());
            var memoryCheckPoint = dotMemory.Check();
            var doc = htmlToPdfGenerator.Generate();

            if (!Directory.Exists("files"))
            {
                Directory.CreateDirectory("files");
            }

            var converter = new PdfConverter(_engine);
#pragma warning disable SEC0112 // Path Tampering Unvalidated File Path
#pragma warning disable SCS0018 // Potential Path Traversal vulnerability was found where '{0}' in '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.
            using var stream = new FileStream(
                Path.Combine(
                    "Files",
                    $"{DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture)}.pdf"),
                FileMode.Create);
#pragma warning restore SCS0018 // Potential Path Traversal vulnerability was found where '{0}' in '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.
#pragma warning restore SEC0112 // Path Tampering Unvalidated File Path
            var converted = await converter.ConvertAsync(
                doc,
                _ => stream,
                CancellationToken.None);
            _output.WriteLine(converted.ToString(CultureInfo.InvariantCulture));

            dotMemory.Check(
                memory =>
                {
                    var objects = memory.GetDifference(memoryCheckPoint)
                        .GetSurvivedObjects()
                        .GetObjects(where => where.Namespace.Like(nameof(AdaskoTheBeAsT)));
                    var objectCount = objects.ObjectsCount;
                    objectCount.Should().BeLessOrEqualTo(5);
                });
        }
    }
}
