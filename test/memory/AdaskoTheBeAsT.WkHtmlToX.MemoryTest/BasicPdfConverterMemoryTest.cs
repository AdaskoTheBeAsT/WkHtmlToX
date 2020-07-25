using System;
using System.Globalization;
using System.IO;
using AdaskoTheBeAsT.WkHtmlToX.Loaders;
using FluentAssertions;
using JetBrains.dotMemoryUnit;
using Xunit;
using Xunit.Abstractions;

namespace AdaskoTheBeAsT.WkHtmlToX.MemoryTest
{
    public class BasicPdfConverterMemoryTest
    {
        private readonly ITestOutputHelper _output;

        public BasicPdfConverterMemoryTest(
            ITestOutputHelper output)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
            DotMemoryUnitTestOutput.SetOutputMethod(_output.WriteLine);
        }

        [DotMemoryUnit(SavingStrategy = SavingStrategy.OnAnyFail)]
        [Fact]
        public void ShouldNotLeaveAnyObjectsSurvived()
        {
            var libFactory = new LibraryLoaderFactory();
            var htmlToPdfGenerator = new HtmlToPdfDocumentGenerator(new SmallHtmlGenerator());
            var memoryCheckPoint = dotMemory.Check();
            using (var libraryLoader = libFactory.Create(
                (int)Environment.OSVersion.Platform,
                runtimeIdentifier: null))
            {
                libraryLoader.Load();
                var doc = htmlToPdfGenerator.Generate();

                if (!Directory.Exists("files"))
                {
                    Directory.CreateDirectory("files");
                }

                using var converter = new BasicPdfConverter();
#pragma warning disable SEC0112 // Path Tampering Unvalidated File Path
                using var stream = new FileStream(
                    Path.Combine(
                        "Files",
                        $"{DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture)}.pdf"),
                    FileMode.Create);
#pragma warning restore SEC0112 // Path Tampering Unvalidated File Path
                var converted = converter.Convert(
                    doc,
                    _ => stream);
                _output.WriteLine(converted.ToString(CultureInfo.InvariantCulture));
            }

            dotMemory.Check(
                memory =>
                {
                    var objects = memory.GetDifference(memoryCheckPoint)
                        .GetSurvivedObjects()
                        .GetObjects(where => where.Namespace.Like("AdaskoTheBeAsT"));
                    var objectCount = objects.ObjectsCount;
                    objectCount.Should().BeLessOrEqualTo(5);
                });
        }
    }
}
