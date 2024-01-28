using System;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.Exceptions;
using AdaskoTheBeAsT.WkHtmlToX.Loaders;
using FluentAssertions;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Loaders;

public sealed class LibraryLoaderFactoryTest
{
    private readonly LibraryLoaderFactory _sut;

    public LibraryLoaderFactoryTest()
    {
        _sut = new LibraryLoaderFactory();
    }

    public static TheoryData<CustomData> GetTestData()
    {
        return new TheoryData<CustomData>(
            new CustomData(
                (int)PlatformID.MacOSX,
                WkHtmlToXRuntimeIdentifier.Ubuntu1804X64,
                typeof(LibraryLoaderOsx)),
            new CustomData(
                (int)PlatformID.Unix,
                WkHtmlToXRuntimeIdentifier.Ubuntu1804X64,
                typeof(LibraryLoaderLinux)),
            new CustomData(
                128,
                WkHtmlToXRuntimeIdentifier.Ubuntu1804X64,
                typeof(LibraryLoaderLinux)),
            new CustomData(
                (int)PlatformID.Win32NT,
                null,
                typeof(LibraryLoaderWindows)),
            new CustomData(
                (int)PlatformID.Win32S,
                null,
                typeof(LibraryLoaderWindows)),
            new CustomData(
                (int)PlatformID.Win32Windows,
                null,
                typeof(LibraryLoaderWindows)),
            new CustomData(
                (int)PlatformID.WinCE,
                null,
                typeof(LibraryLoaderWindows)),
            new CustomData(
                (int)PlatformID.Xbox,
                null,
                typeof(LibraryLoaderWindows)));
    }

    [Fact]
    public void GetModuleShouldThrowWhenNotKnownPlatformIdPassed()
    {
        // Arrange
        var configuration = new WkHtmlToXConfiguration(12345, runtimeIdentifier: null);

        // ReSharper disable once AssignmentIsFullyDiscarded
#pragma warning disable IDISP004 // Don't ignore created IDisposable.
        Action action = () => _ = _sut.Create(configuration);
#pragma warning restore IDISP004 // Don't ignore created IDisposable.

        // Act and Assert
        action.Should().Throw<InvalidPlatformIdentifierException>();
    }

    [Fact]
    public void GetModuleShouldThrowWhenLinuxPlatformIdAndNotKnownRuntimeIdentifierPassed()
    {
        // Arrange
        var configuration = new WkHtmlToXConfiguration((int)PlatformID.Unix, runtimeIdentifier: null);

        // ReSharper disable once AssignmentIsFullyDiscarded
#pragma warning disable IDISP004 // Don't ignore created IDisposable.
        Action action = () => _ = _sut.Create(configuration);
#pragma warning restore IDISP004 // Don't ignore created IDisposable.

        // Act and Assert
        action.Should().Throw<InvalidLinuxRuntimeIdentifierException>();
    }

    [Theory]
    [MemberData(nameof(GetTestData))]
    public void CreateShouldReturnCorrectLoaderAndRuntimeIdentifierPassed(
        CustomData data)
    {
        // Arrange
        var configuration = new WkHtmlToXConfiguration(data.PlatformId, data.RuntimeIdentifier);

        // Act
        using var result = _sut.Create(configuration);

        // Assert
        result.Should().BeOfType(data.Type);
    }

    public class CustomData(
        int platformId,
        WkHtmlToXRuntimeIdentifier? runtimeIdentifier,
        Type type)
    {
        public int PlatformId { get; } = platformId;

        public WkHtmlToXRuntimeIdentifier? RuntimeIdentifier { get; } = runtimeIdentifier;

        public Type Type { get; } = type;
    }
}
