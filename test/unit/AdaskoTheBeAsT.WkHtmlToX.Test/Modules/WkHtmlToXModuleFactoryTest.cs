using System;
using System.Collections.Generic;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
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
                ModuleKind.Pdf,
                typeof(WkHtmlToPdfCommonModule),
            };
            yield return new object[]
            {
                ModuleKind.Image,
                typeof(WkHtmlToImageCommonModule),
            };
        }

        [Theory]
        [MemberData(nameof(GetTestData))]
        public void GetModuleShouldReturnCorrectModuleWhenPlatformAndModuleKindPassed(
            ModuleKind moduleKind,
            Type type)
        {
            // Act
            var result = _sut.GetModule(moduleKind);

            // Assert
            result.Should().BeOfType(type);
        }
    }
}
