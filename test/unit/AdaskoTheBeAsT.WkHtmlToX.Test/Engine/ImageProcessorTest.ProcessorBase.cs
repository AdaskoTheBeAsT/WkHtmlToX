using System;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AutoFixture;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Moq;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Engine;

public partial class ImageProcessorTest
{
    private readonly Fixture _fixture;
    private readonly Mock<IWkHtmlToImageModule> _module;
    private readonly ImageProcessor _sut;

    public ImageProcessorTest()
    {
        _fixture = new Fixture();
        _module = new Mock<IWkHtmlToImageModule>(MockBehavior.Strict);
        var globalSettingsPtr = new IntPtr(_fixture.Create<int>());
        var converterPtr = new IntPtr(_fixture.Create<int>());
        _module.Setup(m =>
                m.CreateGlobalSettings())
            .Returns(globalSettingsPtr);
        _module.Setup(m =>
                m.CreateConverter(It.IsAny<IntPtr>()))
            .Returns(converterPtr);
        _module.Setup(
            m =>
                m.DestroyConverter(It.IsAny<IntPtr>()));
        _module.Setup(
                m =>
                    m.Convert(It.IsAny<IntPtr>()))
            .Returns(value: true);

        var configuration = new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, runtimeIdentifier: null);
        _sut = new ImageProcessor(configuration, _module.Object);
    }

    [Fact]
    public void ShouldThrowExceptionWhenNullPassedInModuleConstructor()
    {
        var configuration = new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, runtimeIdentifier: null);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

        // ReSharper disable once AssignmentIsFullyDiscarded
#pragma warning disable MA0003 // Name parameter
#pragma warning disable IDISP004 // Don't ignore created IDisposable.
        Action action = () => _ = new ImageProcessor(configuration, null);
#pragma warning restore IDISP004 // Don't ignore created IDisposable.
#pragma warning restore MA0003 // Name parameter
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Act and Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ShouldReturnGlobalApplySettingWhenIsGlobalTruePassed()
    {
        // Arrange
        var intVal = _fixture.Create<int>();
        var name = _fixture.Create<string>();
        var value = _fixture.Create<string>();

        // ReSharper disable once ConvertToLocalFunction
        Func<IntPtr, string, string?, int> setGlobalSetting = (_, _, _) => intVal;

        _module.Setup(m => m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()))
            .Returns(setGlobalSetting);

        // Act
        var resultFunc = _sut.GetApplySettingFunc(useGlobal: true);
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

        // ReSharper disable once ConvertToLocalFunction
        Func<IntPtr, string, string?, int> setGlobalSetting = (_, _, _) => intVal;

        _module.Setup(m => m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()))
            .Returns(setGlobalSetting);

        // Act
        var resultFunc = _sut.GetApplySettingFunc(useGlobal: false);
        var result = resultFunc(new IntPtr(1), name, value);

        // Assert
        using (new AssertionScope())
        {
            resultFunc.Should().NotBeNull();
            result.Should().Be(intVal);
        }
    }
}
