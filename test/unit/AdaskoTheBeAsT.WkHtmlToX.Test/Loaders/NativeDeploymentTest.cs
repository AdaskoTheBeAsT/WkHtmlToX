using System;
using System.IO;
using System.Runtime.InteropServices;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.Exceptions;
using AdaskoTheBeAsT.WkHtmlToX.Loaders;
using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Loaders;

public sealed class NativeDeploymentTest
{
    [Theory]
    [InlineData(Architecture.X64, "x64")]
    [InlineData(Architecture.X86, "x86")]
    public void ArchitectureShouldNotUseBitnessAlone(Architecture architecture, string expected) =>
        LibraryLoaderBase.GetProcessorArchitecture(architecture).Should().Be(expected);

    [Theory]
    [InlineData(Architecture.Arm)]
    [InlineData(Architecture.Arm64)]
    public void UnsupportedArchitectureShouldBeRejected(Architecture architecture)
    {
        Action action = () => LibraryLoaderBase.GetProcessorArchitecture(architecture);
        action.Should().Throw<PlatformNotSupportedException>();
    }

    [Theory]
    [InlineData("wkhtmltox.dll")]
    [InlineData("C:wkhtmltox.dll")]
    [InlineData(@"\wkhtmltox.dll")]
    public void RelativeNativePathsShouldBeRejected(string path)
    {
        var factory = new LibraryLoaderFactory();
        var configuration = new WkHtmlToXConfiguration { NativeLibraryPath = path };
        Action action = () =>
        {
            using var loader = factory.Create(configuration);
        };
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MissingExplicitPathShouldNotFallBackToBundledBinary()
    {
        var factory = new LibraryLoaderFactory();
        var configuration = new WkHtmlToXConfiguration
        {
            NativeLibraryPath = Path.Combine(AppContext.BaseDirectory, Guid.NewGuid().ToString("N"), "wkhtmltox.dll"),
        };
        using var loader = factory.Create(configuration);
        Action action = loader.Load;
        action.Should().Throw<DllNotLoadedException>();
    }

    [Fact]
    public void ExplicitLinuxPathShouldNotRequireLegacyDistributionIdentifier()
    {
        var factory = new LibraryLoaderFactory();
        using var loader = factory.Create(new WkHtmlToXConfiguration((int)PlatformID.Unix, runtimeIdentifier: null)
        {
            NativeLibraryPath = Path.Combine(AppContext.BaseDirectory, "libwkhtmltox.so"),
        });
        loader.Should().BeOfType<LibraryLoaderLinux>();
    }
}
