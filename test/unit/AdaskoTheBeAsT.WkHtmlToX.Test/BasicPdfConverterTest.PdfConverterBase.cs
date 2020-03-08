using System;
using System.Collections.Generic;
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
        public void ApplyShouldSetProperBooleanValueInConfig(bool isGlobal, bool value, string expected)
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

        [Theory]
        [InlineData(false, 2, "2")]
        [InlineData(false, 1.11, "1.11")]
        [InlineData(false, 1.2345, "1.23")]
        [InlineData(false, 1.2378, "1.24")]
        [InlineData(true, 2, "2")]
        [InlineData(true, 1.11, "1.11")]
        [InlineData(true, 1.2345, "1.23")]
        [InlineData(true, 1.2378, "1.24")]
        public void ApplyShouldSetProperDoubleValueInConfig(bool isGlobal, double value, string expected)
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

        [Theory]
        [InlineData(false, 2, "2")]
        [InlineData(false, 7, "7")]
        [InlineData(true, 2, "2")]
        [InlineData(true, 7, "7")]
        public void ApplyShouldSetProperIntValueInConfig(bool isGlobal, int value, string expected)
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

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ApplyShouldSetProperDictionaryValueInConfig(bool isGlobal)
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
            var keyName1 = _fixture.Create<string>();
            var value1 = _fixture.Create<string>();
            var keyName2 = _fixture.Create<string>();
            var dictionary = new Dictionary<string, string?>
            {
                [keyName1] = value1,
                [keyName2] = null,
            };

            // Act
            _sut.Apply(intPtr, name, dictionary, isGlobal);

            // Assert
            if (isGlobal)
            {
                _module.Verify(
                    m =>
                        m.SetGlobalSetting(
                            It.Is<IntPtr>(v => v == intPtr),
                            It.Is<string>(v => v == $"{name}.append"),
                            It.Is<string?>(v => v == null)));
                _module.Verify(
                    m =>
                        m.SetGlobalSetting(
                            It.Is<IntPtr>(v => v == intPtr),
                            It.Is<string>(v => v == $"{name}[0]"),
                            It.Is<string?>(v => v == $"{keyName1}\n{value1}")));
                _module.VerifyNoOtherCalls();
            }
            else
            {
                _pdfModule.Verify(
                    m =>
                        m.SetObjectSetting(
                            It.Is<IntPtr>(v => v == intPtr),
                            It.Is<string>(v => v == $"{name}.append"),
                            It.Is<string?>(v => v == null)));
                _pdfModule.Verify(
                    m =>
                        m.SetObjectSetting(
                            It.Is<IntPtr>(v => v == intPtr),
                            It.Is<string>(v => v == $"{name}[0]"),
                            It.Is<string?>(v => v == $"{keyName1}\n{value1}")));
                _pdfModule.VerifyNoOtherCalls();
            }
        }
    }
}
