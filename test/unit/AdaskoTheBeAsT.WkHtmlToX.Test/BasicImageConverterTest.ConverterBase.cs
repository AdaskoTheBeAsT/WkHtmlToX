using System;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test
{
    public partial class BasicImageConverterTest
    {
        [Fact]
        public void ShouldThrowExceptionWhenNullPassedInModuleConstructor()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

            // ReSharper disable once AssignmentIsFullyDiscarded
            Action action = () => _ = new BasicImageConverter(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Act & Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ShouldReturnGlobalApplySettingWhenIsGlobalTruePassed()
        {
            // Arrange
            var intVal = _fixture.Create<int>();
            var name = _fixture.Create<string>();
            var value = _fixture.Create<string>();

            int SetGlobalSetting(
                IntPtr ptr,
                string n,
                string? v) =>
                intVal;

            _module.Setup(m => m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()))
                .Returns((Func<IntPtr, string, string?, int>)SetGlobalSetting);

            // Act
            var resultFunc = _sut.GetApplySettingFunc(true);
            var result = resultFunc(new IntPtr(1), name, value);

            // Assert
            using (new AssertionScope())
            {
                resultFunc.Should().NotBeNull();
                result.Should().Be(intVal);
            }
        }

        [Fact]
        public void ShouldReturnObjectApplySettingWhenIsGlobalFalsePassed()
        {
            // Arrange
            var intVal = _fixture.Create<int>();
            var name = _fixture.Create<string>();
            var value = _fixture.Create<string>();

            int SetGlobalSetting(
                IntPtr ptr,
                string n,
                string? v) =>
                intVal;

            _module.Setup(m => m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()))
                .Returns((Func<IntPtr, string, string?, int>)SetGlobalSetting);

            // Act
            var resultFunc = _sut.GetApplySettingFunc(false);
            var result = resultFunc(new IntPtr(1), name, value);

            // Assert
            using (new AssertionScope())
            {
                resultFunc.Should().NotBeNull();
                result.Should().Be(intVal);
            }
        }
    }
}
