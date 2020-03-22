using System;
using System.IO;
using System.Threading.Tasks;
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
        : IDisposable
    {
        private readonly Fixture _fixture;
        private readonly MockRepository _mockRepository;
        private readonly Mock<IWkHtmlToXModule> _module;
        private readonly Mock<IWkHtmlToPdfModule> _pdfModule;
        private readonly BasicPdfConverter _sut;

        public BasicPdfConverterTest()
        {
            _fixture = new Fixture();
            _mockRepository = new MockRepository(MockBehavior.Loose);

            var moduleFactoryMock = _mockRepository.Create<IWkHtmlToXModuleFactory>();
            _module = _mockRepository.Create<IWkHtmlToXModule>();
            _module.Setup(m => m.Dispose());
            moduleFactoryMock.Setup(mf => mf.GetModule(It.IsAny<ModuleKind>()))
                .Returns(_module.Object);

            var pdfModuleFactoryMock = _mockRepository.Create<IWkHtmlToPdfModuleFactory>();
            _pdfModule = _mockRepository.Create<IWkHtmlToPdfModule>();
            pdfModuleFactoryMock.Setup(mf => mf.GetModule())
                .Returns(_pdfModule.Object);

            _sut = new BasicPdfConverter(moduleFactoryMock.Object, pdfModuleFactoryMock.Object);
        }

        public void Dispose()
        {
            _sut.Dispose();
        }

        [Fact]
        public void ShouldNotThrowWhenUsingDefaultConstructor()
        {
            // Arrange
            Action action = () => _ = new BasicPdfConverter();

            // Act & Assert
            action.Should().NotThrow();
        }

        [Fact]
        public void ConvertShouldReturnNullStreamWhenNotConverted()
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
            var result = _sut.Convert(document);

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
        public void ConvertShouldReturnStreamWhenConverted()
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
            var result = _sut.Convert(document);

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

        [Fact]
        public async Task ConvertAsyncShouldReturnNullStreamWhenNotConverted()
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
            var result = await _sut.ConvertAsync(document);

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
        public async Task ConvertAsyncShouldReturnStreamWhenConverted()
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
            var result = await _sut.ConvertAsync(document);

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

        [Fact]
        public void ConvertAsyncShouldShouldThrowExceptionWhenSomethingInvalid()
        {
            // Arrange
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Func<Task> func = async () => await _sut.ConvertAsync(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Act & Assert
            func.Should().Throw<ArgumentNullException>();
        }
    }
}
