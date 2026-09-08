using System;
using System.IO;
using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.Utils;
using AutoFixture;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Moq;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Engine;

public partial class ImageProcessorTest
{
    [Fact]
    public void CreateConverterShouldThrowArgumentNullExceptionWhenNullPassed()
    {
        // Arrange
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

        // ReSharper disable once AssignmentIsFullyDiscarded
        Action action = () => _ = _sut.CreateConverter(document: null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Act and Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreateConverterShouldInvokeCreateGlobalSettings()
    {
        // Arrange
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
                m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()))
            .Returns(1);
        var document = new HtmlToImageDocument();

        // Act
        var result = _sut.CreateConverter(document);

        // Assert
        using (new AssertionScope())
        {
            _module.Verify(m => m.CreateGlobalSettings(), Times.Once);
            _module.Verify(
                m =>
                    m.SetGlobalSetting(
                        It.IsAny<IntPtr>(),
                        It.IsAny<string>(),
                        It.IsAny<string?>()),
                Times.Never);
            result.converterPtr.Should().Be(converterPtr);
            result.globalSettingsPtr.Should().Be(globalSettingsPtr);
        }
    }

    [Fact]
    public void CreateConverterShouldSetGlobalSettings()
    {
        // Arrange
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
                m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()))
            .Returns(1);
        var document = new HtmlToImageDocument();
        var quality = _fixture.Create<string>();
        document.ImageSettings.Quality = quality;

        // Act
        var result = _sut.CreateConverter(document);

        // Assert
        using (new AssertionScope())
        {
            _module.Verify(m => m.CreateGlobalSettings(), Times.Once);
            _module.Verify(
                m =>
                    m.SetGlobalSetting(
                        It.Is<IntPtr>(v => v == globalSettingsPtr),
                        It.Is<string>(v => v == nameof(quality)),
                        It.Is<string?>(v => v == quality)),
                Times.Once);
            result.converterPtr.Should().Be(converterPtr);
            result.globalSettingsPtr.Should().Be(globalSettingsPtr);
        }
    }

    [Fact]
    public void CreateConverterShouldDestroyGlobalSettingsWhenApplyingSettingsThrows()
    {
        // Arrange
        var globalSettingsPtr = new IntPtr(_fixture.Create<int>());
        var expectedException = new InvalidOperationException();
        _module.Setup(m => m.CreateGlobalSettings())
            .Returns(globalSettingsPtr);
        _module.Setup(m => m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()))
            .Throws(expectedException);
        _module.Setup(m => m.DestroyGlobalSetting(It.IsAny<IntPtr>()));

        var document = new HtmlToImageDocument();
        document.ImageSettings.Quality = _fixture.Create<string>();

        // Act
        Action action = () => _sut.CreateConverter(document);

        // Assert
        using (new AssertionScope())
        {
            action.Should().Throw<InvalidOperationException>().Which.Should().BeSameAs(expectedException);
            _module.Verify(m => m.DestroyGlobalSetting(globalSettingsPtr), Times.Once);
            _module.Verify(m => m.CreateConverter(It.IsAny<IntPtr>()), Times.Never);
        }
    }

    [Fact]
    public void CreateConverterShouldDestroyGlobalSettingsWhenConverterPointerIsZero()
    {
        // Arrange
        var globalSettingsPtr = new IntPtr(_fixture.Create<int>());
        _module.Setup(m => m.CreateGlobalSettings())
            .Returns(globalSettingsPtr);
        _module.Setup(m => m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()))
            .Returns(1);
        _module.Setup(m => m.CreateConverter(It.IsAny<IntPtr>()))
            .Returns(IntPtr.Zero);
        _module.Setup(m => m.DestroyGlobalSetting(It.IsAny<IntPtr>()));

        var document = new HtmlToImageDocument();

        // Act
        Action action = () => _sut.CreateConverter(document);

        // Assert
        using (new AssertionScope())
        {
            action.Should().Throw<ArgumentException>();
            _module.Verify(m => m.DestroyGlobalSetting(globalSettingsPtr), Times.Once);
        }
    }

    [Fact]
    public void ConvertImplShouldThrowExceptionWhenNullImageSettingsPassed()
    {
        // Arrange
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        var document = new HtmlToImageDocument
        {
            ImageSettings = null,
        };
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        Action action = () => _sut.Convert(document, _ => Stream.Null);

        // Act and Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ConvertImplShouldThrowExceptionWhenModuleInitializeNotEqualOne()
    {
        // Arrange
        var document = new HtmlToImageDocument();
        _module.Setup(m => m.Initialize(It.IsAny<int>()))
            .Returns(0);

        _module.Setup(m =>
                m.CreateConverter(It.IsAny<IntPtr>()))
            .Returns(IntPtr.Zero);
        _module.Setup(m => m.DestroyGlobalSetting(It.IsAny<IntPtr>()));

        _module.Setup(m =>
               m.GetOutput(It.IsAny<IntPtr>(), It.IsAny<Func<int, Stream>>()));

        Action action = () => _sut.Convert(document, _ => Stream.Null);

        // Act and Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ConvertShouldThrowExceptionWhenNullDocumentPassed()
    {
        // Arrange
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Action action = () => _sut.Convert(document: null, _ => Stream.Null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Act and Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ConvertShouldThrowExceptionWhenNullCreateStreamFuncPassed()
    {
        // Arrange
        var document = new HtmlToImageDocument();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Action action = () => _sut.Convert(document, createStreamFunc: null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Act and Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ConvertShouldReturnNullStreamWhenNotConverted()
    {
        // Arrange
        var globalSettingsPtr = new IntPtr(_fixture.Create<int>());
        var converterPtr = new IntPtr(_fixture.Create<int>());
        _module.Setup(m => m.Initialize(It.IsAny<int>()))
            .Returns(1);
        _module.Setup(m =>
                m.CreateGlobalSettings())
            .Returns(globalSettingsPtr);
        _module.Setup(m =>
                m.CreateConverter(It.IsAny<IntPtr>()))
            .Returns(converterPtr);
        _module.Setup(
            m =>
                m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()))
            .Returns(1);
        _module.Setup(m => m.Convert(It.IsAny<IntPtr>()))
            .Returns(value: false);
        _module.Setup(m => m.GetOutput(It.IsAny<IntPtr>(), It.IsAny<Func<int, Stream>>()));
        _module.Setup(m => m.DestroyGlobalSetting(It.IsAny<IntPtr>()));
        _module.Setup(m => m.DestroyConverter(It.IsAny<IntPtr>()));
        var document = new HtmlToImageDocument();
        var quality = _fixture.Create<string>();
        document.ImageSettings.Quality = quality;

        // Act
        var result = _sut.Convert(document, _ => Stream.Null);

        // Assert
        using (new AssertionScope())
        {
            _module.Verify(m => m.CreateGlobalSettings(), Times.Once);
            _module.Verify(
                m =>
                    m.SetGlobalSetting(
                        It.Is<IntPtr>(v => v == globalSettingsPtr),
                        It.Is<string>(v => v == nameof(quality)),
                        It.Is<string?>(v => v == quality)),
                Times.Once);
            _module.Verify(m => m.GetOutput(It.IsAny<IntPtr>(), It.IsAny<Func<int, Stream>>()), Times.Never);
            _module.Verify(m => m.DestroyGlobalSetting(It.IsAny<IntPtr>()), Times.Never);
            _module.Verify(m => m.DestroyConverter(It.IsAny<IntPtr>()), Times.Once);
            result.Should().BeFalse();
        }
    }

    [Fact]
    public void ConvertShouldReturnStreamWhenConverted()
    {
        // Arrange
        using var memoryStream = new MemoryStream();
        var globalSettingsPtr = new IntPtr(_fixture.Create<int>());
        var converterPtr = new IntPtr(_fixture.Create<int>());
        _module.Setup(m => m.Initialize(It.IsAny<int>()))
            .Returns(1);
        _module.Setup(m =>
                m.CreateGlobalSettings())
            .Returns(globalSettingsPtr);
        _module.Setup(m =>
                m.CreateConverter(It.IsAny<IntPtr>()))
            .Returns(converterPtr);
        _module.Setup(
            m =>
                m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()))
            .Returns(1);
        _module.Setup(m => m.Convert(It.IsAny<IntPtr>()))
            .Returns(value: true);
        _module.Setup(m => m.GetOutput(It.IsAny<IntPtr>(), It.IsAny<Func<int, Stream>>()));
        _module.Setup(m => m.DestroyGlobalSetting(It.IsAny<IntPtr>()));
        _module.Setup(m => m.DestroyConverter(It.IsAny<IntPtr>()));
        var document = new HtmlToImageDocument();
        var quality = _fixture.Create<string>();
        document.ImageSettings.Quality = quality;

        // Act
        // ReSharper disable once AccessToDisposedClosure
#pragma warning disable IDISP011
        var result = _sut.Convert(document, _ => memoryStream);
#pragma warning restore IDISP011

        // Assert
        using (new AssertionScope())
        {
            _module.Verify(m => m.CreateGlobalSettings(), Times.Once);
            _module.Verify(
                m =>
                    m.SetGlobalSetting(
                        It.Is<IntPtr>(v => v == globalSettingsPtr),
                        It.Is<string>(v => v == nameof(quality)),
                        It.Is<string?>(v => v == quality)),
                Times.Once);
            _module.Verify(m => m.GetOutput(It.IsAny<IntPtr>(), It.IsAny<Func<int, Stream>>()), Times.Once);
            _module.Verify(m => m.DestroyGlobalSetting(It.IsAny<IntPtr>()), Times.Never);
            _module.Verify(m => m.DestroyConverter(It.IsAny<IntPtr>()), Times.Once);
            result.Should().BeTrue();
        }
    }

    [Fact]
    public void ConvertShouldReleaseRegisteredCallbacksAfterConverterTeardown()
    {
        // Arrange
        var globalSettingsPtr = new IntPtr(_fixture.Create<int>());
        var converterPtr = new IntPtr(_fixture.Create<int>());
        _module.Setup(m => m.Initialize(It.IsAny<int>()))
            .Returns(1);
        _module.Setup(m => m.CreateGlobalSettings())
            .Returns(globalSettingsPtr);
        _module.Setup(m => m.CreateConverter(It.IsAny<IntPtr>()))
            .Returns(converterPtr);
        _module.Setup(m => m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()))
            .Returns(1);
        _module.Setup(m => m.SetErrorCallback(It.IsAny<IntPtr>(), It.IsAny<StringCallback>()));
        _module.Setup(m => m.Convert(It.IsAny<IntPtr>()))
            .Returns(value: true);
        _module.Setup(m => m.GetOutput(It.IsAny<IntPtr>(), It.IsAny<Func<int, Stream>>()));
        _module.Setup(m => m.DestroyConverter(It.IsAny<IntPtr>()));

        var configuration = new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, runtimeIdentifier: null)
        {
            ErrorAction = _ => { },
        };

        var sut = new ImageProcessor(configuration, _module.Object);

        // Act
        var result = sut.Convert(new HtmlToImageDocument(), _ => Stream.Null);

        // Assert
        using (new AssertionScope())
        {
            result.Should().BeTrue();
            sut.HasRegisteredCallbacks.Should().BeFalse();
        }
    }
}
