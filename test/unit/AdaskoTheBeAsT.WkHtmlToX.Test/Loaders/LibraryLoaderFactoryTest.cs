using System;
using System.Collections.Generic;
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
            // ReSharper disable once AssignmentIsFullyDiscarded
            Action action = () => _ = _sut.Create(12345, null);

            // Act & Assert
            action.Should().Throw<InvalidPlatformIdentifierException>();
        }

        [Fact]
        public void GetModuleShouldThrowWhenLinuxPlatformIdAndNotKnownRuntimeIdentifierPassed()
        {
            // Arrange
            // ReSharper disable once AssignmentIsFullyDiscarded
            Action action = () => _ = _sut.Create((int)PlatformID.Unix, null);

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
            // Act
            var result = _sut.Create(platformId, runtimeIdentifier);

            // Assert
            result.Should().BeOfType(type);
        }
    }
}
