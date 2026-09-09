using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.Settings;

namespace AdaskoTheBeAsT.WkHtmlToX.NativeMemoryProbe;

internal static class Program
{
    private const string ResultPrefix = "RESULT:";
    private const string SmallHtml =
        """
        <html><body><p>This paragraph contains enough content for memory probing.</p><p>Repeatable content keeps the rendering path stable.</p></body></html>
        """;

#pragma warning disable VSTHRD200,CC0061
    public static async Task<int> Main(string[] args)
#pragma warning restore VSTHRD200,CC0061
    {
        try
        {
            var options = ProbeOptions.Parse(args);
            var result = await RunAsync(options).ConfigureAwait(false);
            await Console.Out.WriteLineAsync(ResultPrefix + JsonSerializer.Serialize(result)).ConfigureAwait(false);
            return 0;
        }
        catch (Exception exception)
        {
            await Console.Error.WriteLineAsync(exception.ToString()).ConfigureAwait(false);
            return 1;
        }
    }

    private static async Task<NativeMemoryProbeResult> RunAsync(ProbeOptions options)
    {
        var samplePrivateBytes = new List<long>(options.MeasurementIterations);
        var htmlFilePath = CreateTemporaryHtmlFile();

        try
        {
            var engine = new WkHtmlToXEngine(new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, runtimeIdentifier: null));
            await using (engine.ConfigureAwait(false))
            {
                await engine.InitializeAsync().ConfigureAwait(false);

                await RunWarmupAsync(engine, options.Mode, htmlFilePath, options.WarmupIterations)
                    .ConfigureAwait(false);

                var baselinePrivateBytes = TakePrivateBytesSample();

                for (var i = 0; i < options.MeasurementIterations; i++)
                {
                    await ConvertAsync(engine, options.Mode, htmlFilePath).ConfigureAwait(false);
                    samplePrivateBytes.Add(TakePrivateBytesSample());
                }

                var finalPrivateBytes = samplePrivateBytes.LastOrDefault(baselinePrivateBytes);
                var peakPrivateBytes = samplePrivateBytes.Count == 0
                    ? baselinePrivateBytes
                    : Math.Max(baselinePrivateBytes, samplePrivateBytes.Max());

                return new NativeMemoryProbeResult
                {
                    Mode = options.Mode,
                    WarmupIterations = options.WarmupIterations,
                    MeasurementIterations = options.MeasurementIterations,
                    BaselinePrivateBytes = baselinePrivateBytes,
                    FinalPrivateBytes = finalPrivateBytes,
                    PeakPrivateBytes = peakPrivateBytes,
                    GrowthBytes = finalPrivateBytes - baselinePrivateBytes,
                    SamplePrivateBytes = [.. samplePrivateBytes],
                };
            }
        }
        finally
        {
            DeleteTemporaryFile(htmlFilePath);
        }
    }

    private static async Task RunWarmupAsync(WkHtmlToXEngine engine, string mode, string htmlFilePath, int count)
    {
        for (var i = 0; i < count; i++)
        {
            await ConvertAsync(engine, mode, htmlFilePath).ConfigureAwait(false);
        }
    }

    private static Task ConvertAsync(WkHtmlToXEngine engine, string mode, string htmlFilePath) =>
        string.Equals(mode, "image", StringComparison.OrdinalIgnoreCase)
            ? ConvertImageAsync(engine, htmlFilePath)
            : ConvertPdfAsync(engine);

    private static async Task ConvertPdfAsync(WkHtmlToXEngine engine)
    {
        var converter = new PdfConverter(engine);
        var document = new HtmlToPdfDocument
        {
            GlobalSettings =
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Landscape,
            },
            ObjectSettings =
            {
                new PdfObjectSettings
                {
                    PagesCount = true,
                    HtmlContent = SmallHtml,
                    WebSettings =
                    {
                        DefaultEncoding = "utf-8",
                    },
                },
            },
        };

        MemoryStream? stream = null;

#pragma warning disable IDISP011
        var converted = await converter.ConvertAsync(
            document,
            _ =>
            {
#pragma warning disable CC0022
                stream ??= new MemoryStream();
#pragma warning restore CC0022
                return stream;
            },
            CancellationToken.None)
            .ConfigureAwait(false);
#pragma warning restore IDISP011

#pragma warning disable MA0004 // Use Task.ConfigureAwait
        await using var outputStream = stream ?? throw new InvalidOperationException("PDF conversion did not provide an output stream.");
#pragma warning restore MA0004 // Use Task.ConfigureAwait
        EnsureSuccessfulConversion(converted, outputStream.Length, "pdf");
    }

    private static async Task ConvertImageAsync(WkHtmlToXEngine engine, string htmlFilePath)
    {
        var converter = new ImageConverter(engine);
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

        MemoryStream? stream = null;

#pragma warning disable IDISP011
        var converted = await converter.ConvertAsync(
            document,
            _ =>
            {
#pragma warning disable CC0022
                stream ??= new MemoryStream();
#pragma warning restore CC0022
                return stream;
            },
            CancellationToken.None)
            .ConfigureAwait(false);
#pragma warning restore IDISP011

#pragma warning disable MA0004 // Use Task.ConfigureAwait
        await using var outputStream = stream ?? throw new InvalidOperationException("Image conversion did not provide an output stream.");
#pragma warning restore MA0004 // Use Task.ConfigureAwait
        EnsureSuccessfulConversion(converted, outputStream.Length, "image");
    }

    private static void EnsureSuccessfulConversion(bool converted, long outputLength, string mode)
    {
        if (!converted)
        {
            throw new InvalidOperationException($"{mode} conversion failed during memory probing.");
        }

        if (outputLength <= 0)
        {
            throw new InvalidOperationException($"{mode} conversion produced no output during memory probing.");
        }
    }

    private static long TakePrivateBytesSample()
    {
        ForceGarbageCollection();

        using var process = Process.GetCurrentProcess();
        process.Refresh();
        return process.PrivateMemorySize64;
    }

    private static void ForceGarbageCollection()
    {
#pragma warning disable S1215
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
#pragma warning restore S1215
    }

    private static string CreateTemporaryHtmlFile()
    {
        var htmlFilePath = Path.Combine(Path.GetTempPath(), $"wkhtmltox-native-probe-{Guid.NewGuid():N}.html");
#pragma warning disable SEC0116
#pragma warning disable SCS0018
        File.WriteAllText(htmlFilePath, SmallHtml);
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

    private sealed class ProbeOptions
    {
        public string Mode { get; private set; } = "pdf";

        public int WarmupIterations { get; private set; } = 3;

        public int MeasurementIterations { get; private set; } = 15;

        public static ProbeOptions Parse(string[] args)
        {
            var options = new ProbeOptions();
            var index = 0;

            while (index < args.Length)
            {
                var argument = args[index];
                if (string.Equals(argument, "--mode", StringComparison.OrdinalIgnoreCase) && index + 1 < args.Length)
                {
                    options.Mode = args[index + 1];
                    index += 2;
                    continue;
                }

                if (string.Equals(argument, "--warmup", StringComparison.OrdinalIgnoreCase) && index + 1 < args.Length)
                {
                    options.WarmupIterations = int.Parse(args[index + 1], CultureInfo.InvariantCulture);
                    index += 2;
                    continue;
                }

                if (string.Equals(argument, "--measure", StringComparison.OrdinalIgnoreCase) && index + 1 < args.Length)
                {
                    options.MeasurementIterations = int.Parse(args[index + 1], CultureInfo.InvariantCulture);
                    index += 2;
                    continue;
                }

                index++;
            }

            if (!string.Equals(options.Mode, "pdf", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(options.Mode, "image", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentOutOfRangeException(nameof(args), options.Mode, "Mode must be either 'pdf' or 'image'.");
            }

            if (options.WarmupIterations < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(args), options.WarmupIterations, "Warmup iterations cannot be negative.");
            }

            if (options.MeasurementIterations <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(args), options.MeasurementIterations, "Measurement iterations must be positive.");
            }

            return options;
        }
    }

    private sealed class NativeMemoryProbeResult
    {
        public string Mode { get; set; } = string.Empty;

        public int WarmupIterations { get; set; }

        public int MeasurementIterations { get; set; }

        public long BaselinePrivateBytes { get; set; }

        public long FinalPrivateBytes { get; set; }

        public long PeakPrivateBytes { get; set; }

        public long GrowthBytes { get; set; }

        public long[] SamplePrivateBytes { get; set; } = [];
    }
}
