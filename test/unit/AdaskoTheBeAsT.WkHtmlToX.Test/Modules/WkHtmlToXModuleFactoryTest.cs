using System;
using System.Collections.Generic;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Exceptions;
using AdaskoTheBeAsT.WkHtmlToX.Modules;
using FluentAssertions;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Modules
{
    public sealed class WkHtmlToXModuleFactoryTest
    {
        private readonly WkHtmlToXModuleFactory _sut;

        public WkHtmlToXModuleFactoryTest()
        {
            _sut = new WkHtmlToXModuleFactory();
        }

        public static IEnumerable<object[]> GetTestData()
        {
            yield return new object[]
            {
                PlatformID.MacOSX,
                ModuleKind.Pdf,
                typeof(WkHtmlToPdfPosixCommonModule),
            };
            yield return new object[]
            {
                PlatformID.MacOSX,
                ModuleKind.Image,
                typeof(WkHtmlToImagePosixCommonModule),
            };
            yield return new object[]
            {
                PlatformID.Unix,
                ModuleKind.Pdf,
                typeof(WkHtmlToPdfPosixCommonModule),
            };
            yield return new object[]
            {
                PlatformID.Unix,
                ModuleKind.Image,
                typeof(WkHtmlToImagePosixCommonModule),
            };
            yield return new object[]
            {
                128,
                ModuleKind.Pdf,
                typeof(WkHtmlToPdfPosixCommonModule),
            };
            yield return new object[]
            {
                128,
                ModuleKind.Image,
                typeof(WkHtmlToImagePosixCommonModule),
            };

            yield return new object[]
            {
                PlatformID.Win32NT,
                ModuleKind.Pdf,
                typeof(WkHtmlToPdfWindowsCommonModule),
            };
            yield return new object[]
            {
                PlatformID.Win32NT,
                ModuleKind.Image,
                typeof(WkHtmlToImageWindowsCommonModule),
            };
            yield return new object[]
            {
                PlatformID.Win32S,
                ModuleKind.Pdf,
                typeof(WkHtmlToPdfWindowsCommonModule),
            };
            yield return new object[]
            {
                PlatformID.Win32S,
                ModuleKind.Image,
                typeof(WkHtmlToImageWindowsCommonModule),
            };
            yield return new object[]
            {
                PlatformID.Win32Windows,
                ModuleKind.Pdf,
                typeof(WkHtmlToPdfWindowsCommonModule),
            };
            yield return new object[]
            {
                PlatformID.Win32Windows,
                ModuleKind.Image,
                typeof(WkHtmlToImageWindowsCommonModule),
            };
            yield return new object[]
            {
                PlatformID.WinCE,
                ModuleKind.Pdf,
                typeof(WkHtmlToPdfWindowsCommonModule),
            };
            yield return new object[]
            {
                PlatformID.WinCE,
                ModuleKind.Image,
                typeof(WkHtmlToImageWindowsCommonModule),
            };
            yield return new object[]
            {
                PlatformID.Xbox,
                ModuleKind.Pdf,
                typeof(WkHtmlToPdfWindowsCommonModule),
            };
            yield return new object[]
            {
                PlatformID.Xbox,
                ModuleKind.Image,
                typeof(WkHtmlToImageWindowsCommonModule),
            };
        }

        [Fact]
        public void GetModuleShouldThrowWhenNotKnownPlatformIdPassed()
        {
            // Arrange
            // ReSharper disable once AssignmentIsFullyDiscarded
            Action action = () => _ = _sut.GetModule(12345, ModuleKind.Image);

            // Act & Assert
            action.Should().Throw<InvalidPlatformIdentifierException>();
        }

        [Theory]
        [MemberData(nameof(GetTestData))]
        public void GetModuleShouldReturnCorrectModuleWhenPlatformAndModuleKindPassed(
            int platformId,
            ModuleKind moduleKind,
            Type type)
        {
            // Act
            var result = _sut.GetModule(platformId, moduleKind);

            // Assert
            result.Should().BeOfType(type);
        }
    }
}
