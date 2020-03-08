using System;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AutoFixture;
using FluentAssertions;
using Moq;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test
{
    public sealed partial class BasicPdfConverterTest
    {
        [Fact]
        public void ShouldThrowExceptionWhenNullPassedInPdfModuleConstructor()
        {
            // Arrange
            var moduleMock = new Mock<IWkHtmlToXModuleFactory>();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action action = () => _ = new BasicPdfConverter(moduleMock.Object, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Act & Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData(false, true, "true")]
        [InlineData(false, false, "false")]
        [InlineData(true, true, "true")]
        [InlineData(true, false, "false")]
        public void ApplyShouldSetProperBooleanValueInGlobalConfig(bool isGlobal, bool value, string expected)
        {
            // Arrange
            if (isGlobal)
            {
                _module.Setup(m => m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()));
            }
            else
            {
                _pdfModule.Setup(m => m.SetObjectSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()));
            }

            var intPtr = new IntPtr(_fixture.Create<int>());
            var name = _fixture.Create<string>();

            // Act
            _sut.Apply(intPtr, name, value, isGlobal);

            // Assert
            if (isGlobal)
            {
                _module.Verify(
                    m =>
                        m.SetGlobalSetting(
                            It.Is<IntPtr>(v => v == intPtr),
                            It.Is<string>(v => v == name),
                            It.Is<string?>(v => v == expected)));
            }
            else
            {
                _pdfModule.Verify(
                    m =>
                        m.SetObjectSetting(
                            It.Is<IntPtr>(v => v == intPtr),
                            It.Is<string>(v => v == name),
                            It.Is<string?>(v => v == expected)));
            }
        }
    }
}
