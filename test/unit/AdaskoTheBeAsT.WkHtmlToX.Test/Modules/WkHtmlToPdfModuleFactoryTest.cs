using System;
using System.Collections.Generic;
using AdaskoTheBeAsT.WkHtmlToX.Modules;
using FluentAssertions;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Modules
{
    public sealed class WkHtmlToPdfModuleFactoryTest
    {
        private readonly WkHtmlToPdfModuleFactory _sut;

        public WkHtmlToPdfModuleFactoryTest()
        {
            _sut = new WkHtmlToPdfModuleFactory();
        }

        public static IEnumerable<object[]> GetTestData()
        {
            yield return new object[]
            {
                PlatformID.MacOSX,
                typeof(WkHtmlToPdfPosixAdditionalModule),
            };
            yield return new object[]
            {
                PlatformID.Unix,
                typeof(WkHtmlToPdfPosixAdditionalModule),
            };
            yield return new object[]
            {
                128,
                typeof(WkHtmlToPdfPosixAdditionalModule),
            };
            yield return new object[]
            {
                PlatformID.Win32NT,
                typeof(WkHtmlToPdfWindowsAdditionalModule),
            };
            yield return new object[]
            {
                PlatformID.Win32S,
                typeof(WkHtmlToPdfWindowsAdditionalModule),
            };
            yield return new object[]
            {
                PlatformID.Win32Windows,
                typeof(WkHtmlToPdfWindowsAdditionalModule),
            };
            yield return new object[]
            {
                PlatformID.WinCE,
                typeof(WkHtmlToPdfWindowsAdditionalModule),
            };
            yield return new object[]
            {
                PlatformID.Xbox,
                typeof(WkHtmlToPdfWindowsAdditionalModule),
            };
            yield return new object[]
            {
                12345,
                typeof(WkHtmlToPdfWindowsAdditionalModule),
            };
        }

        [Theory]
        [MemberData(nameof(GetTestData))]
        public void GetModuleShouldReturnCorrectModuleWhenPlatformPassed(int platformId, Type type)
        {
            // Act
            var result = _sut.GetModule(platformId);

            // Assert
            result.Should().BeOfType(type);
        }
    }
}
