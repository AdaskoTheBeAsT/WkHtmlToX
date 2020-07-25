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
using Moq;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test
{
    public sealed partial class BasicPdfConverterTest
        : IDisposable
    {
        private readonly Fixture _fixture;
        private readonly Mock<IWkHtmlToXModule> _module;
        private readonly Mock<IWkHtmlToPdfModule> _pdfModule;
        private readonly BasicPdfConverter _sut;

        public BasicPdfConverterTest()
        {
            _fixture = new Fixture();

            var moduleFactoryMock = new Mock<IWkHtmlToXModuleFactory>();
            _module = new Mock<IWkHtmlToXModule>();
            _module.Setup(m => m.Dispose());
            moduleFactoryMock.Setup(mf => mf.GetModule(It.IsAny<ModuleKind>()))
                .Returns(_module.Object);

            _pdfModule = new Mock<IWkHtmlToPdfModule>();

            _sut = new BasicPdfConverter(moduleFactoryMock.Object, _pdfModule.Object);
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
#pragma warning disable IDISP004 // Don't ignore created IDisposable.
            Action action = () => _ = new BasicPdfConverter();
#pragma warning restore IDISP004 // Don't ignore created IDisposable.

            // Act & Assert
            action.Should().NotThrow();
        }

        [Fact]
        public void ConvertShouldThrowExceptionWhenNullDocumentPassed()
        {
            // Arrange
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action action = () => _sut.Convert(null, _ => Stream.Null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Act & Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ConvertShouldThrowExceptionWhenNullCreateStreamPassed()
        {
            // Arrange
            var document = new HtmlToPdfDocument();
            document.ObjectSettings.Add(new PdfObjectSettings());
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action action = () => _sut.Convert(document, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Act & Assert
            action.Should().Throw<ArgumentNullException>();
        }

#pragma warning disable MA0051 // Method is too long
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
            var result = _sut.Convert(document, _ => Stream.Null);

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
            }
        }
#pragma warning restore MA0051 // Method is too long

#pragma warning disable MA0051 // Method is too long
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
            // ReSharper disable once AccessToDisposedClosure
            var result = _sut.Convert(document, _ => memoryStream);

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
#pragma warning restore MA0051 // Method is too long

#pragma warning disable MA0051 // Method is too long
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

            // Act
            var result = await _sut.ConvertAsync(document, _ => Stream.Null, CancellationToken.None);

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
            }
        }
#pragma warning restore MA0051 // Method is too long

#pragma warning disable MA0051 // Method is too long
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
#pragma warning restore MA0051 // Method is too long

        [Fact]
        public void ConvertAsyncShouldShouldThrowExceptionWhenSomethingInvalid()
        {
            // Arrange
            var document = new HtmlToPdfDocument();
            Func<Task> func = async () =>
                await _sut.ConvertAsync(document, _ => Stream.Null, CancellationToken.None)
                    .ConfigureAwait(false);

            // Act & Assert
            func.Should().Throw<ArgumentException>();
        }
    }
}
