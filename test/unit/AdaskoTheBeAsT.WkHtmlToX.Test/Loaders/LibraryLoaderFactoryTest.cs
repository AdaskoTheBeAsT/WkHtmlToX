using System;
using System.Collections.Generic;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.Exceptions;
using AdaskoTheBeAsT.WkHtmlToX.Loaders;
using FluentAssertions;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Loaders
{
    public sealed class LibraryLoaderFactoryTest
    {
        private readonly LibraryLoaderFactory _sut;

        public LibraryLoaderFactoryTest()
        {
            _sut = new LibraryLoaderFactory();
        }

        public static IEnumerable<object?[]> GetTestData()
        {
            yield return new object?[]
            {
                PlatformID.MacOSX,
                WkHtmlToXRuntimeIdentifier.Ubuntu1804X64,
                typeof(LibraryLoaderOsx),
            };
            yield return new object?[]
            {
                PlatformID.Unix,
                WkHtmlToXRuntimeIdentifier.Ubuntu1804X64,
                typeof(LibraryLoaderLinux),
            };
            yield return new object?[]
            {
                128,
                WkHtmlToXRuntimeIdentifier.Ubuntu1804X64,
                typeof(LibraryLoaderLinux),
            };
            yield return new object?[]
            {
                PlatformID.Win32NT,
                null,
                typeof(LibraryLoaderWindows),
            };
            yield return new object?[]
            {
                PlatformID.Win32S,
                null,
                typeof(LibraryLoaderWindows),
            };
            yield return new object?[]
            {
                PlatformID.Win32Windows,
                null,
                typeof(LibraryLoaderWindows),
            };
            yield return new object?[]
            {
                PlatformID.WinCE,
                null,
                typeof(LibraryLoaderWindows),
            };
            yield return new object?[]
            {
                PlatformID.Xbox,
                null,
                typeof(LibraryLoaderWindows),
            };
        }

        [Fact]
        public void GetModuleShouldThrowWhenNotKnownPlatformIdPassed()
        {
            // Arrange
            var configuration = new WkHtmlToXConfiguration(12345, null);

            // ReSharper disable once AssignmentIsFullyDiscarded
#pragma warning disable IDISP004 // Don't ignore created IDisposable.
            Action action = () => _ = _sut.Create(configuration);
#pragma warning restore IDISP004 // Don't ignore created IDisposable.

            // Act & Assert
            action.Should().Throw<InvalidPlatformIdentifierException>();
        }

        [Fact]
        public void GetModuleShouldThrowWhenLinuxPlatformIdAndNotKnownRuntimeIdentifierPassed()
        {
            // Arrange
            var configuration = new WkHtmlToXConfiguration((int)PlatformID.Unix, null);

            // ReSharper disable once AssignmentIsFullyDiscarded
#pragma warning disable IDISP004 // Don't ignore created IDisposable.
            Action action = () => _ = _sut.Create(configuration);
#pragma warning restore IDISP004 // Don't ignore created IDisposable.

            // Act & Assert
            action.Should().Throw<InvalidLinuxRuntimeIdentifierException>();
        }

        [Theory]
        [MemberData(nameof(GetTestData))]
        public void CreateShouldReturnCorrectLoaderAndRuntimeIdentifierPassed(
            int platformId,
            WkHtmlToXRuntimeIdentifier? runtimeIdentifier,
            Type type)
        {
            // Arrange
            var configuration = new WkHtmlToXConfiguration(platformId, runtimeIdentifier);

            // Act
            using var result = _sut.Create(configuration);

            // Assert
            result.Should().BeOfType(type);
        }
    }
}
