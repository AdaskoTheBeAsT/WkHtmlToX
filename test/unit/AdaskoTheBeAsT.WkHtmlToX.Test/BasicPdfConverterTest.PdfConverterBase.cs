using System;
using System.IO;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AdaskoTheBeAsT.WkHtmlToX.Settings;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test
{
    public sealed partial class BasicPdfConverterTest
    {
        [Fact]
        public void PdfModuleConstructorShouldThrowExceptionWhenNullPassed()
        {
            // Arrange
            var moduleMock = new Mock<IWkHtmlToXModuleFactory>();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action action = () => _ = new BasicPdfConverter(moduleMock.Object, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Act & Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GetApplySettingFuncShouldReturnGlobalApplySettingWhenIsGlobalTruePassed()
        {
            // Arrange
            var intVal1 = _fixture.Create<int>();
            var intVal2 = _fixture.Create<int>();
            var name = _fixture.Create<string>();
            var value = _fixture.Create<string>();
            Func<IntPtr, string, string?, int> setGlobalSetting = (intptr, name, value) => intVal1;
            Func<IntPtr, string, string?, int> setObjectSetting = (intptr, name, value) => intVal2;

            _module.Setup(m => m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()))
                    .Returns(setGlobalSetting);
            _pdfModule.Setup(m => m.SetObjectSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()))
                .Returns(setObjectSetting);

            // Act
            var resultFunc = _sut.GetApplySettingFunc(true);
            var result = resultFunc(new IntPtr(1), name, value);

            // Assert
            using (new AssertionScope())
            {
                resultFunc.Should().NotBeNull();
                result.Should().Be(intVal1);
            }
        }

        [Fact]
        public void GetApplySettingFuncShouldReturnObjectApplySettingWhenIsGlobalFalsePassed()
        {
            // Arrange
            var intVal1 = _fixture.Create<int>();
            var intVal2 = _fixture.Create<int>();
            var name = _fixture.Create<string>();
            var value = _fixture.Create<string>();
            Func<IntPtr, string, string?, int> setGlobalSetting = (intptr, name, value) => intVal1;
            Func<IntPtr, string, string?, int> setObjectSetting = (intptr, name, value) => intVal2;

            _module.Setup(m => m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()))
                .Returns(setGlobalSetting);
            _pdfModule.Setup(m => m.SetObjectSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()))
                .Returns(setObjectSetting);

            // Act
            var resultFunc = _sut.GetApplySettingFunc(false);
            var result = resultFunc(new IntPtr(1), name, value);

            // Assert
            using (new AssertionScope())
            {
                resultFunc.Should().NotBeNull();
                result.Should().Be(intVal2);
            }
        }

        [Fact]
        public void CreateConverterShouldThrowArgumentNullExceptionWhenNullPassed()
        {
            // Arrange
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action action = () => _ = _sut.CreateConverter(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Act & Assert
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
                    m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()));
            _pdfModule.Setup(
                m =>
                    m.SetObjectSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()));
            var document = new HtmlToPdfDocument();

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
                _pdfModule.Verify(
                    m =>
                        m.SetObjectSetting(
                            It.IsAny<IntPtr>(),
                            It.IsAny<string>(),
                            It.IsAny<string?>()),
                    Times.Never);
                result.converterPtr.Should().Be(converterPtr);
                result.globalSettingsPtr.Should().Be(globalSettingsPtr);
                result.objectSettingsPtrs.Should().NotBeNull();
                result.objectSettingsPtrs.Should().BeEmpty();
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
                    m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()));
            _pdfModule.Setup(
                m =>
                    m.SetObjectSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()));
            var document = new HtmlToPdfDocument();
            var documentTitle = _fixture.Create<string>();
            document.GlobalSettings.DocumentTitle = documentTitle;

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
                            It.Is<string>(v => v == "documentTitle"),
                            It.Is<string?>(v => v == documentTitle)),
                    Times.Once);
                _pdfModule.Verify(
                    m =>
                        m.SetObjectSetting(
                            It.IsAny<IntPtr>(),
                            It.IsAny<string>(),
                            It.IsAny<string?>()),
                    Times.Never);
                result.converterPtr.Should().Be(converterPtr);
                result.globalSettingsPtr.Should().Be(globalSettingsPtr);
                result.objectSettingsPtrs.Should().NotBeNull();
                result.objectSettingsPtrs.Should().BeEmpty();
            }
        }

        [Fact]
        public void CreateConverterShouldSetObjectSettings()
        {
            // Arrange
            var globalSettingsPtr = new IntPtr(_fixture.Create<int>());
            var objectSettingsPtr = new IntPtr(_fixture.Create<int>());
            var converterPtr = new IntPtr(_fixture.Create<int>());
            _module.Setup(m =>
                    m.CreateGlobalSettings())
                .Returns(globalSettingsPtr);
            _module.Setup(m =>
                    m.CreateConverter(It.IsAny<IntPtr>()))
                .Returns(converterPtr);
            _module.Setup(
                m =>
                    m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()));
            _pdfModule.Setup(m =>
                    m.CreateObjectSettings())
                .Returns(objectSettingsPtr);
            _pdfModule.Setup(
                m =>
                    m.SetObjectSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()));
            var document = new HtmlToPdfDocument();
            var documentTitle = _fixture.Create<string>();
            var captionText = _fixture.Create<string>();
            document.GlobalSettings.DocumentTitle = documentTitle;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            document.ObjectSettings.Add(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            document.ObjectSettings.Add(
                new PdfObjectSettings
                {
                    CaptionText = captionText,
                    HtmlContent = "<html><head><title>title</title></head><body></body></html>",
                });

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
                            It.Is<string>(v => v == "documentTitle"),
                            It.Is<string?>(v => v == documentTitle)),
                    Times.Once);
                _pdfModule.Verify(m => m.CreateObjectSettings(), Times.Once);
                _pdfModule.Verify(
                    m =>
                        m.SetObjectSetting(
                            It.Is<IntPtr>(v => v == objectSettingsPtr),
                            It.Is<string>(v => v == "toc.captionText"),
                            It.Is<string?>(v => v == captionText)),
                    Times.Once);
                result.converterPtr.Should().Be(converterPtr);
                result.globalSettingsPtr.Should().Be(globalSettingsPtr);
                result.objectSettingsPtrs.Should().NotBeNull();
                result.objectSettingsPtrs.Should().HaveCount(1);
                result.objectSettingsPtrs[0].Should().Be(objectSettingsPtr);
            }
        }

        [Fact]
        public void ConvertImplShouldThrowExceptionWhenNullDocumentPassed()
        {
            // Arrange
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action action = () => _sut.ConvertImpl(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Act & Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ConvertImplShouldThrowExceptionWhenObjectSettingsListEmpty()
        {
            // Arrange
            var document = new HtmlToPdfDocument();
            Action action = () => _sut.ConvertImpl(document);

            // Act & Assert
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ConvertImplShouldThrowExceptionWhenModuleInitializeNotEqualOne()
        {
            // Arrange
            var document = new HtmlToPdfDocument();
            document.ObjectSettings.Add(new PdfObjectSettings());
            _module.Setup(m => m.Initialize(It.IsAny<int>()))
                .Returns(0);

            Action action = () => _sut.ConvertImpl(document);

            // Act & Assert
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ConvertImplShouldReturnNullStreamWhenNotConverted()
        {
            // Arrange
            var globalSettingsPtr = new IntPtr(_fixture.Create<int>());
            var objectSettingsPtr = new IntPtr(_fixture.Create<int>());
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
                    m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()));
            _module.Setup(m => m.Convert(It.IsAny<IntPtr>()))
                .Returns(false);
            _module.Setup(m => m.GetOutput(It.IsAny<IntPtr>()));
            _module.Setup(m => m.DestroyGlobalSetting(It.IsAny<IntPtr>()));
            _module.Setup(m => m.DestroyConverter(It.IsAny<IntPtr>()));
            _module.Setup(m => m.Terminate());
            _pdfModule.Setup(m =>
                    m.CreateObjectSettings())
                .Returns(objectSettingsPtr);
            _pdfModule.Setup(
                m =>
                    m.SetObjectSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()));
            _pdfModule.Setup(m => m.DestroyObjectSetting(It.IsAny<IntPtr>()));
            var document = new HtmlToPdfDocument();
            var documentTitle = _fixture.Create<string>();
            var captionText = _fixture.Create<string>();
            document.GlobalSettings.DocumentTitle = documentTitle;
            document.ObjectSettings.Add(
                new PdfObjectSettings
                {
                    CaptionText = captionText,
                    HtmlContent = "<html><head><title>title</title></head><body></body></html>",
                });

            // Act
            var result = _sut.ConvertImpl(document);

            // Assert
            using (new AssertionScope())
            {
                _module.Verify(m => m.Initialize(It.IsAny<int>()), Times.Once);
                _module.Verify(m => m.CreateGlobalSettings(), Times.Once);
                _module.Verify(
                    m =>
                        m.SetGlobalSetting(
                            It.Is<IntPtr>(v => v == globalSettingsPtr),
                            It.Is<string>(v => v == "documentTitle"),
                            It.Is<string?>(v => v == documentTitle)),
                    Times.Once);
                _pdfModule.Verify(m => m.CreateObjectSettings(), Times.Once);
                _module.Verify(m => m.GetOutput(It.IsAny<IntPtr>()), Times.Never);
                _pdfModule.Verify(
                    m =>
                        m.SetObjectSetting(
                            It.Is<IntPtr>(v => v == objectSettingsPtr),
                            It.Is<string>(v => v == "toc.captionText"),
                            It.Is<string?>(v => v == captionText)),
                    Times.Once);
                _pdfModule.Verify(m => m.DestroyObjectSetting(It.IsAny<IntPtr>()), Times.Once);
                _module.Verify(m => m.DestroyGlobalSetting(It.IsAny<IntPtr>()), Times.Once);
                _module.Verify(m => m.DestroyConverter(It.IsAny<IntPtr>()), Times.Once);
                _module.Verify(m => m.Terminate(), Times.Once);
                result.Should().Be(Stream.Null);
            }
        }

        [Fact]
        public void ConvertImplShouldReturnStreamWhenConverted()
        {
            // Arrange
            using var memoryStream = new MemoryStream();
            var globalSettingsPtr = new IntPtr(_fixture.Create<int>());
            var objectSettingsPtr = new IntPtr(_fixture.Create<int>());
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
                    m.SetGlobalSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()));
            _module.Setup(m => m.Convert(It.IsAny<IntPtr>()))
                .Returns(true);
            _module.Setup(m => m.GetOutput(It.IsAny<IntPtr>()))
                .Returns(memoryStream);
            _module.Setup(m => m.DestroyGlobalSetting(It.IsAny<IntPtr>()));
            _module.Setup(m => m.DestroyConverter(It.IsAny<IntPtr>()));
            _module.Setup(m => m.Terminate());
            _pdfModule.Setup(m =>
                    m.CreateObjectSettings())
                .Returns(objectSettingsPtr);
            _pdfModule.Setup(
                m =>
                    m.SetObjectSetting(It.IsAny<IntPtr>(), It.IsAny<string>(), It.IsAny<string?>()));
            _pdfModule.Setup(m => m.DestroyObjectSetting(It.IsAny<IntPtr>()));
            var document = new HtmlToPdfDocument();
            var documentTitle = _fixture.Create<string>();
            var captionText = _fixture.Create<string>();
            document.GlobalSettings.DocumentTitle = documentTitle;
            document.ObjectSettings.Add(
                new PdfObjectSettings
                {
                    CaptionText = captionText,
                    HtmlContent = "<html><head><title>title</title></head><body></body></html>",
                });

            // Act
            var result = _sut.ConvertImpl(document);

            // Assert
            using (new AssertionScope())
            {
                _module.Verify(m => m.Initialize(It.IsAny<int>()), Times.Once);
                _module.Verify(m => m.CreateGlobalSettings(), Times.Once);
                _module.Verify(
                    m =>
                        m.SetGlobalSetting(
                            It.Is<IntPtr>(v => v == globalSettingsPtr),
                            It.Is<string>(v => v == "documentTitle"),
                            It.Is<string?>(v => v == documentTitle)),
                    Times.Once);
                _pdfModule.Verify(m => m.CreateObjectSettings(), Times.Once);
                _module.Verify(m => m.GetOutput(It.IsAny<IntPtr>()), Times.Once);
                _pdfModule.Verify(
                    m =>
                        m.SetObjectSetting(
                            It.Is<IntPtr>(v => v == objectSettingsPtr),
                            It.Is<string>(v => v == "toc.captionText"),
                            It.Is<string?>(v => v == captionText)),
                    Times.Once);
                _pdfModule.Verify(m => m.DestroyObjectSetting(It.IsAny<IntPtr>()), Times.Once);
                _module.Verify(m => m.DestroyGlobalSetting(It.IsAny<IntPtr>()), Times.Once);
                _module.Verify(m => m.DestroyConverter(It.IsAny<IntPtr>()), Times.Once);
                _module.Verify(m => m.Terminate(), Times.Once);
                result.Should().Be(memoryStream);
            }
        }
    }
}
