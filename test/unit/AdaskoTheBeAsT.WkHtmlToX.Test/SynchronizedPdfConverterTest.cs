using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AdaskoTheBeAsT.WkHtmlToX.Settings;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.IO;
using Moq;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test
{
    public sealed class SynchronizedPdfConverterTest
        : IDisposable
    {
        private readonly Fixture _fixture;
        private readonly Mock<IWkHtmlToXModule> _module;
        private readonly Mock<IWkHtmlToPdfModule> _pdfModule;
        private readonly SynchronizedPdfConverter _sut;

        public SynchronizedPdfConverterTest()
        {
            _fixture = new Fixture();

            var moduleFactoryMock = new Mock<IWkHtmlToXModuleFactory>();
            _module = new Mock<IWkHtmlToXModule>();
            _module.Setup(m => m.Dispose());
            moduleFactoryMock.Setup(mf => mf.GetModule(It.IsAny<ModuleKind>()))
                .Returns(_module.Object);

            _pdfModule = new Mock<IWkHtmlToPdfModule>();

            _sut = new SynchronizedPdfConverter(moduleFactoryMock.Object, _pdfModule.Object);
        }

        public void Dispose()
        {
            _sut.Dispose();
        }

        [Fact]
        public void ShouldNotThrowWhenUsingDefaultConstructor()
        {
            // Arrange
            // ReSharper disable once AssignmentIsFullyDiscarded
            Action action = () => _ = new SynchronizedPdfConverter();

            // Act & Assert
            action.Should().NotThrow();
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
            _module.Setup(m => m.GetOutput(It.IsAny<IntPtr>(), It.IsAny<Func<int, Stream>>()));
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

            var recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();

            // Act
            Stream? stream = null;
            var result = await _sut.ConvertAsync(
                document,
                length =>
                {
                    stream = recyclableMemoryStreamManager.GetStream(
                        Guid.NewGuid(),
                        "wkhtmltox",
                        length);
                    return stream;
                },
                CancellationToken.None);

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
                _module.Verify(m => m.GetOutput(It.IsAny<IntPtr>(), It.IsAny<Func<int, Stream>>()), Times.Never);
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
                result.Should().BeFalse();
#if NETCOREAPP3_1
                if (stream != null)
                {
                    await stream.DisposeAsync();
                }
#else
                stream?.Dispose();
#endif
            }
        }

        [Fact]
        public async Task ConvertAsyncShouldReturnStreamWhenConverted()
        {
            // Arrange
#if NETCOREAPP3_1
            await using var memoryStream = new MemoryStream();
#else
            using var memoryStream = new MemoryStream();
#endif
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
            _module.Setup(m => m.GetOutput(It.IsAny<IntPtr>(), It.IsAny<Func<int, Stream>>()));
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
            var result = await _sut.ConvertAsync(document, _ => memoryStream, CancellationToken.None);

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
                _module.Verify(m => m.GetOutput(It.IsAny<IntPtr>(), It.IsAny<Func<int, Stream>>()), Times.Once);
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
                result.Should().BeTrue();
            }
        }

        [Fact]
        public void ConvertAsyncShouldShouldThrowExceptionWhenSomethingInvalid()
        {
            // Arrange
            var document = new HtmlToPdfDocument();
            Func<Task> func = async () =>
                await _sut.ConvertAsync(document, _ => Stream.Null, CancellationToken.None);

            // Act & Assert
            func.Should().Throw<ArgumentException>();
        }
    }
}
