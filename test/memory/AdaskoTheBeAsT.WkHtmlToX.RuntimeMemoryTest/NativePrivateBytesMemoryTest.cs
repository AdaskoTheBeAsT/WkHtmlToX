using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.RuntimeMemoryTest;

public sealed class NativePrivateBytesMemoryTest
{
    private const int WarmupIterationCount = 3;
    private const int MeasurementIterationCount = 15;

    private readonly ITestOutputHelper _output;

    public NativePrivateBytesMemoryTest(ITestOutputHelper output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    [Theory]
    [InlineData("pdf", 16777216)]
    [InlineData("image", 16777216)]
    public async Task ShouldKeepNativePrivateBytesBoundedInChildProcessAsync(string mode, long maxAllowedGrowthBytes)
    {
        var probeAssemblyPath = GetProbeAssemblyPath();
        var cancellationToken = TestContext.Current.CancellationToken;
        File.Exists(probeAssemblyPath).Should().BeTrue();

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
        };

        process.StartInfo.ArgumentList.Add("exec");
        process.StartInfo.ArgumentList.Add(probeAssemblyPath);
        process.StartInfo.ArgumentList.Add("--mode");
        process.StartInfo.ArgumentList.Add(mode);
        process.StartInfo.ArgumentList.Add("--warmup");
        process.StartInfo.ArgumentList.Add(WarmupIterationCount.ToString());
        process.StartInfo.ArgumentList.Add("--measure");
        process.StartInfo.ArgumentList.Add(MeasurementIterationCount.ToString());

        process.Start();

        var standardOutputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var standardErrorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        var standardOutput = await standardOutputTask;
        var standardError = await standardErrorTask;

        _output.WriteLine($"Probe stdout for {mode}:{Environment.NewLine}{standardOutput}");
        if (!string.IsNullOrWhiteSpace(standardError))
        {
            _output.WriteLine($"Probe stderr for {mode}:{Environment.NewLine}{standardError}");
        }

        process.ExitCode.Should().Be(0);

        var probeResult = ParseProbeResult(standardOutput);

        using (new AssertionScope())
        {
            probeResult.Mode.Should().Be(mode);
            probeResult.SamplePrivateBytes.Should().HaveCount(MeasurementIterationCount);
            probeResult.FinalPrivateBytes.Should().BeGreaterThan(0);
            probeResult.PeakPrivateBytes.Should().BeGreaterThanOrEqualTo(probeResult.FinalPrivateBytes);
            probeResult.GrowthBytes.Should().BeLessThanOrEqualTo(maxAllowedGrowthBytes);
        }
    }

    private static NativeMemoryProbeResult ParseProbeResult(string standardOutput)
    {
        var resultLine = standardOutput
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .LastOrDefault(line => line.StartsWith("RESULT:", StringComparison.Ordinal));

        resultLine.Should().NotBeNullOrWhiteSpace();

        var payload = resultLine!["RESULT:".Length..];
        var probeResult = JsonSerializer.Deserialize<NativeMemoryProbeResult>(payload);
        probeResult.Should().NotBeNull();
        return probeResult!;
    }

    private static string GetProbeAssemblyPath()
    {
        var assemblyDirectoryPath = Path.GetDirectoryName(typeof(NativePrivateBytesMemoryTest).Assembly.Location)
            ?? throw new InvalidOperationException("Cannot determine the test assembly directory.");
        var frameworkDirectory = new DirectoryInfo(assemblyDirectoryPath);
        var configurationDirectory = frameworkDirectory.Parent
            ?? throw new InvalidOperationException("Cannot determine the build configuration directory.");

        return Path.GetFullPath(
            Path.Combine(
                assemblyDirectoryPath,
                "..",
                "..",
                "..",
                "..",
                "AdaskoTheBeAsT.WkHtmlToX.NativeMemoryProbe",
                "bin",
                configurationDirectory.Name,
                frameworkDirectory.Name,
                "AdaskoTheBeAsT.WkHtmlToX.NativeMemoryProbe.dll"));
    }

    private sealed record NativeMemoryProbeResult(
        string Mode,
        long FinalPrivateBytes,
        long PeakPrivateBytes,
        long GrowthBytes,
        long[] SamplePrivateBytes);
}
