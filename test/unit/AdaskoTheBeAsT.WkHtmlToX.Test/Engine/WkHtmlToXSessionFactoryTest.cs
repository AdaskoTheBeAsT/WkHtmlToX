using System;
using System.Threading;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.Exceptions;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Moq;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Engine;

public sealed class WkHtmlToXSessionFactoryTest
{
    private readonly WkHtmlToXConfiguration _configuration;
    private readonly Mock<ILibraryLoader> _libraryLoaderMock;
    private readonly Mock<ILibraryLoaderFactory> _libraryLoaderFactoryMock;
    private readonly Mock<IPdfProcessor> _pdfProcessorMock;
    private readonly Mock<IImageProcessor> _imageProcessorMock;
    private readonly Mock<IWkHtmlToPdfModule> _pdfModuleMock;
    private readonly Mock<IWkHtmlToImageModule> _imageModuleMock;
    private readonly WkHtmlToXSessionFactory _sut;

    public WkHtmlToXSessionFactoryTest()
    {
        _configuration = new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, runtimeIdentifier: null);

        _libraryLoaderMock = new Mock<ILibraryLoader>(MockBehavior.Strict);
        _libraryLoaderMock.Setup(l => l.Dispose());
        _libraryLoaderFactoryMock = new Mock<ILibraryLoaderFactory>(MockBehavior.Strict);
        _libraryLoaderFactoryMock
            .Setup(l => l.Create(It.IsAny<WkHtmlToXConfiguration>()))
            .Returns(_libraryLoaderMock.Object);

        _pdfModuleMock = new Mock<IWkHtmlToPdfModule>(MockBehavior.Strict);
        _pdfProcessorMock = new Mock<IPdfProcessor>(MockBehavior.Strict);
        _pdfProcessorMock.SetupGet(p => p.PdfModule).Returns(_pdfModuleMock.Object);

        _imageModuleMock = new Mock<IWkHtmlToImageModule>(MockBehavior.Strict);
        _imageProcessorMock = new Mock<IImageProcessor>(MockBehavior.Strict);
        _imageProcessorMock.SetupGet(p => p.ImageModule).Returns(_imageModuleMock.Object);

        _sut = new WkHtmlToXSessionFactory(
            _configuration,
            _libraryLoaderFactoryMock.Object,
            () => _pdfProcessorMock.Object,
            () => _imageProcessorMock.Object,
            new NativeRuntimeOwnership());
    }

    [Fact]
    public void CreateSessionShouldLoadLibraryAndInitializeModules()
    {
        // Arrange
        _libraryLoaderMock.Setup(l => l.Load());
        _pdfModuleMock.Setup(p => p.Initialize(It.IsAny<int>())).Returns(1);
        _pdfModuleMock.Setup(p => p.Terminate()).Returns(1);
        _imageModuleMock.Setup(p => p.Initialize(It.IsAny<int>())).Returns(1);
        _imageModuleMock.Setup(p => p.Terminate()).Returns(1);

        // Act
        var session = _sut.CreateSession(CancellationToken.None);

        try
        {
            // Assert
            using (new AssertionScope())
            {
                session.Should().NotBeNull();
                session.Loader.Should().BeSameAs(_libraryLoaderMock.Object);
                session.PdfProcessor.Should().BeSameAs(_pdfProcessorMock.Object);
                session.ImageProcessor.Should().BeSameAs(_imageProcessorMock.Object);
                _libraryLoaderMock.Verify(l => l.Load(), Times.Once);
                _pdfModuleMock.Verify(p => p.Initialize(It.IsAny<int>()), Times.Once);
                _imageModuleMock.Verify(p => p.Initialize(It.IsAny<int>()), Times.Once);
            }
        }
        finally
        {
            _sut.DisposeSession(session);
        }
    }

    [Fact]
    public void CreateSessionShouldThrowWhenPdfInitializationFails()
    {
        // Arrange
        _libraryLoaderMock.Setup(l => l.Load());
        _pdfModuleMock.Setup(p => p.Initialize(It.IsAny<int>())).Returns(0);

        // Act
        Action action = () => _sut.CreateSession(CancellationToken.None);

        // Assert
        action.Should().Throw<PdfModuleInitializationException>();
        _libraryLoaderMock.Verify(l => l.Dispose(), Times.Once);
    }

    [Fact]
    public void CreateSessionShouldThrowWhenImageInitializationFails()
    {
        // Arrange
        _libraryLoaderMock.Setup(l => l.Load());
        _pdfModuleMock.Setup(p => p.Initialize(It.IsAny<int>())).Returns(1);
        _pdfModuleMock.Setup(p => p.Terminate()).Returns(1);
        _imageModuleMock.Setup(p => p.Initialize(It.IsAny<int>())).Returns(0);

        // Act
        Action action = () => _sut.CreateSession(CancellationToken.None);

        // Assert
        using (new AssertionScope())
        {
            action.Should().Throw<ImageModuleInitializationException>();
            _pdfModuleMock.Verify(p => p.Terminate(), Times.Once);
            _libraryLoaderMock.Verify(l => l.Dispose(), Times.Once);
        }
    }

    [Fact]
    public void DisposeSessionShouldTerminateModulesAndReleaseLoader()
    {
        // Arrange
        _libraryLoaderMock.Setup(l => l.Load());
        _pdfModuleMock.Setup(p => p.Initialize(It.IsAny<int>())).Returns(1);
        _pdfModuleMock.Setup(p => p.Terminate()).Returns(1);
        _imageModuleMock.Setup(p => p.Initialize(It.IsAny<int>())).Returns(1);
        _imageModuleMock.Setup(p => p.Terminate()).Returns(1);

        var session = _sut.CreateSession(CancellationToken.None);

        // Act
        _sut.DisposeSession(session);

        // Assert
        using (new AssertionScope())
        {
            _pdfModuleMock.Verify(p => p.Terminate(), Times.Once);
            _imageModuleMock.Verify(p => p.Terminate(), Times.Once);
            _libraryLoaderMock.Verify(l => l.Dispose(), Times.Once);
        }
    }

    [Fact]
    public void CreateSessionShouldRejectMultipleActiveSessions()
    {
        // Arrange
        _libraryLoaderMock.Setup(l => l.Load());
        _pdfModuleMock.Setup(p => p.Initialize(It.IsAny<int>())).Returns(1);
        _pdfModuleMock.Setup(p => p.Terminate()).Returns(1);
        _imageModuleMock.Setup(p => p.Initialize(It.IsAny<int>())).Returns(1);
        _imageModuleMock.Setup(p => p.Terminate()).Returns(1);

        var firstSession = _sut.CreateSession(CancellationToken.None);
        try
        {
            Action createSecond = () => _sut.CreateSession(CancellationToken.None);
            createSecond.Should().Throw<InvalidOperationException>();

            // Assert
            using (new AssertionScope())
            {
                _pdfModuleMock.Verify(p => p.Initialize(It.IsAny<int>()), Times.Once);
                _imageModuleMock.Verify(p => p.Initialize(It.IsAny<int>()), Times.Once);
                _libraryLoaderMock.Verify(l => l.Load(), Times.Once);
            }
        }
        finally
        {
            _sut.DisposeSession(firstSession);
        }
    }

    [Fact]
    public void DisposeSessionShouldThrowWhenNullSessionPassed()
    {
        // Arrange
        Action action = () => _sut.DisposeSession(session: null!);

        // Act and Assert
        action.Should().Throw<ArgumentNullException>();
    }
}
