using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.Exceptions;
using AdaskoTheBeAsT.WkHtmlToX.WorkItems;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Moq;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Engine;

public sealed class EngineTest
    : IDisposable
{
    private readonly WkHtmlToXConfiguration _configuration;
    private readonly Mock<ILibraryLoader> _libraryLoaderMock;
    private readonly Mock<ILibraryLoaderFactory> _libraryLoaderFactoryMock;
    private readonly Mock<IPdfProcessor> _pdfProcessorMock;
    private readonly Mock<IImageProcessor> _imageProcessorMock;
    private readonly Mock<IWkHtmlToPdfModule> _pdfModuleMock;
    private readonly Mock<IWkHtmlToImageModule> _imageModuleMock;
    private readonly WkHtmlToXEngine _sut;

    public EngineTest()
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
        _pdfProcessorMock.SetupGet(p => p.PdfModule)
            .Returns(_pdfModuleMock.Object);

        _imageModuleMock = new Mock<IWkHtmlToImageModule>(MockBehavior.Strict);
        _imageProcessorMock = new Mock<IImageProcessor>(MockBehavior.Strict);
        _imageProcessorMock.SetupGet(p => p.ImageModule)
            .Returns(_imageModuleMock.Object);

        _sut = new WkHtmlToXEngine(
            _configuration,
            _libraryLoaderFactoryMock.Object,
            _pdfProcessorMock.Object,
            _imageProcessorMock.Object);
    }

    public void Dispose() => _sut.Dispose();

    [Fact]
    public void ShouldThrowExceptionWhenNullConfigurationPassed()
    {
        // Arrange
        Action action = () =>
        {
            using var engine = new WkHtmlToXEngine(
                null!,
                new Mock<ILibraryLoaderFactory>(MockBehavior.Strict).Object,
                new Mock<IPdfProcessor>(MockBehavior.Strict).Object,
                new Mock<IImageProcessor>(MockBehavior.Strict).Object);
        };

        // Act and Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ShouldThrowExceptionWhenNullLibraryLoaderFactoryPassed()
    {
        // Arrange
        var configuration = new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, runtimeIdentifier: null);
        Action action = () =>
        {
            using var engine = new WkHtmlToXEngine(
                configuration,
                null!,
                new Mock<IPdfProcessor>(MockBehavior.Strict).Object,
                new Mock<IImageProcessor>(MockBehavior.Strict).Object);
        };

        // Act and Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ShouldThrowExceptionWhenNullPdfProcessorPassed()
    {
        // Arrange
        var configuration = new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, runtimeIdentifier: null);
        Action action = () =>
        {
            using var engine = new WkHtmlToXEngine(
                configuration,
                new Mock<ILibraryLoaderFactory>(MockBehavior.Strict).Object,
                null!,
                new Mock<IImageProcessor>(MockBehavior.Strict).Object);
        };

        // Act and Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ShouldThrowExceptionWhenNullImageProcessorPassed()
    {
        // Arrange
        var configuration = new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, runtimeIdentifier: null);
        Action action = () =>
        {
            using var engine = new WkHtmlToXEngine(
                configuration,
                new Mock<ILibraryLoaderFactory>(MockBehavior.Strict).Object,
                new Mock<IPdfProcessor>(MockBehavior.Strict).Object,
                null!);
        };

        // Act and Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ProcessShouldThrowExceptionWhenNullCancellationTokenPassed()
    {
        // Arrange
        Action action = () => _sut.Process(obj: null);

        // Act and Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ProcessShouldThrowExceptionWhenNotCancellationTokenPassed()
    {
        // Arrange
        Action action = () => _sut.Process(new object());

        // Act and Assert
        action.Should().Throw<Exception>();
    }

    [Fact]
    public void ProcessShouldInvokeInitialization()
    {
        // Arrange
        WkHtmlToXConfiguration? result = null;
        _libraryLoaderFactoryMock
            .Setup(l => l.Create(It.IsAny<WkHtmlToXConfiguration>()))
            .Callback<WkHtmlToXConfiguration>(c => result = c)
            .Returns(_libraryLoaderMock.Object);
        _libraryLoaderMock.Setup(l => l.Load());

        _pdfModuleMock.Setup(p => p.Initialize(It.IsAny<int>()))
            .Returns(1);
        _imageModuleMock.Setup(p => p.Initialize(It.IsAny<int>()))
            .Returns(1);

        // Act
        using var source = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        _sut.Process(source.Token);

        // Assert
        using (new AssertionScope())
        {
            result.Should().Be(_configuration);
            _libraryLoaderMock.Verify(l => l.Load(), Times.Once);
            _pdfModuleMock.Verify(l => l.Initialize(It.IsAny<int>()), Times.Once);
            _imageModuleMock.Verify(l => l.Initialize(It.IsAny<int>()), Times.Once);
        }
    }

    [Fact]
    public void ProcessShouldConsumeItemsFromQueue()
    {
        // Arrange
        _libraryLoaderFactoryMock
            .Setup(l => l.Create(It.IsAny<WkHtmlToXConfiguration>()))
            .Returns(_libraryLoaderMock.Object);
        _libraryLoaderMock.Setup(l => l.Load());

        _pdfModuleMock.Setup(p => p.Initialize(It.IsAny<int>()))
            .Returns(1);
        _imageModuleMock.Setup(p => p.Initialize(It.IsAny<int>()))
            .Returns(1);

        _pdfProcessorMock
            .Setup(p => p.Convert(It.IsAny<HtmlToPdfDocument>(), It.IsAny<Func<int, Stream>>()))
            .Returns(value: true);
        _imageProcessorMock
            .Setup(p => p.Convert(It.IsAny<HtmlToImageDocument>(), It.IsAny<Func<int, Stream>>()))
            .Returns(value: true);
        var pdfConvertWorkItem = new PdfConvertWorkItem(new HtmlToPdfDocument(), _ => Stream.Null);
        var imageConvertWorkItem = new ImageConvertWorkItem(new HtmlToImageDocument(), _ => Stream.Null);

        // Act
        _sut.AddConvertWorkItem(pdfConvertWorkItem, CancellationToken.None);
        _sut.AddConvertWorkItem(imageConvertWorkItem, CancellationToken.None);
        using var source = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        _sut.Process(source.Token);

        // Assert
        using (new AssertionScope())
        {
            _libraryLoaderMock.Verify(l => l.Load(), Times.Once);
            _pdfModuleMock.Verify(l => l.Initialize(It.IsAny<int>()), Times.Once);
            _imageModuleMock.Verify(l => l.Initialize(It.IsAny<int>()), Times.Once);
            _pdfProcessorMock.Verify(
                p => p.Convert(It.IsAny<HtmlToPdfDocument>(), It.IsAny<Func<int, Stream>>()), Times.Once);
            _imageProcessorMock.Verify(
                p => p.Convert(It.IsAny<HtmlToImageDocument>(), It.IsAny<Func<int, Stream>>()), Times.Once);
        }
    }

    [Fact]
    public async Task ProcessShouldConsumeItemsFromQueueAndThrowExceptionAsync()
    {
        // Arrange
        _libraryLoaderFactoryMock
            .Setup(l => l.Create(It.IsAny<WkHtmlToXConfiguration>()))
            .Returns(_libraryLoaderMock.Object);
        _libraryLoaderMock.Setup(l => l.Load());

        _pdfModuleMock.Setup(p => p.Initialize(It.IsAny<int>()))
            .Returns(1);
        _imageModuleMock.Setup(p => p.Initialize(It.IsAny<int>()))
            .Returns(1);

        _pdfProcessorMock
            .Setup(p => p.Convert(It.IsAny<HtmlToPdfDocument>(), It.IsAny<Func<int, Stream>>()))
            .Throws<ArgumentOutOfRangeException>();
        _imageProcessorMock
            .Setup(p => p.Convert(It.IsAny<HtmlToImageDocument>(), It.IsAny<Func<int, Stream>>()))
            .Throws<ArgumentOutOfRangeException>();
        var pdfConvertWorkItem = new PdfConvertWorkItem(new HtmlToPdfDocument(), _ => Stream.Null);
        var imageConvertWorkItem = new ImageConvertWorkItem(new HtmlToImageDocument(), _ => Stream.Null);

        // Act
        _sut.AddConvertWorkItem(pdfConvertWorkItem, CancellationToken.None);
        _sut.AddConvertWorkItem(imageConvertWorkItem, CancellationToken.None);
        using var source = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        _sut.Process(source.Token);

#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
        Func<Task<bool>> action1 = async () => await pdfConvertWorkItem.TaskCompletionSource.Task;
        Func<Task<bool>> action2 = async () => await imageConvertWorkItem.TaskCompletionSource.Task;
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks

        // Assert
        using (new AssertionScope())
        {
            _libraryLoaderMock.Verify(l => l.Load(), Times.Once);
            _pdfModuleMock.Verify(l => l.Initialize(It.IsAny<int>()), Times.Once);
            _imageModuleMock.Verify(l => l.Initialize(It.IsAny<int>()), Times.Once);
            _pdfProcessorMock.Verify(
                p => p.Convert(It.IsAny<HtmlToPdfDocument>(), It.IsAny<Func<int, Stream>>()), Times.Once);
            _imageProcessorMock.Verify(
                p => p.Convert(It.IsAny<HtmlToImageDocument>(), It.IsAny<Func<int, Stream>>()), Times.Once);
            await action1.Should().ThrowAsync<ArgumentOutOfRangeException>();
            await action2.Should().ThrowAsync<ArgumentOutOfRangeException>();
        }
    }

    [Fact]
    public void InitializeInProcessingThreadShouldWork()
    {
        // Arrange
        WkHtmlToXConfiguration? result = null;
        _libraryLoaderFactoryMock
            .Setup(l => l.Create(It.IsAny<WkHtmlToXConfiguration>()))
            .Callback<WkHtmlToXConfiguration>(c => result = c)
            .Returns(_libraryLoaderMock.Object);
        _libraryLoaderMock.Setup(l => l.Load());

        _pdfModuleMock.Setup(p => p.Initialize(It.IsAny<int>()))
            .Returns(1);
        _imageModuleMock.Setup(p => p.Initialize(It.IsAny<int>()))
            .Returns(1);

        // Act
        _sut.InitializeInProcessingThread();

        // Assert
        using (new AssertionScope())
        {
            result.Should().Be(_configuration);
            _libraryLoaderMock.Verify(l => l.Load(), Times.Once);
            _pdfModuleMock.Verify(l => l.Initialize(It.IsAny<int>()), Times.Once);
            _imageModuleMock.Verify(l => l.Initialize(It.IsAny<int>()), Times.Once);
        }
    }

    [Fact]
    public void InitializeInProcessingThreadShouldThrowExceptionWhenPdfInitializationFailed()
    {
        // Arrange
        _libraryLoaderMock.Setup(l => l.Load());

        _pdfModuleMock.Setup(p => p.Initialize(It.IsAny<int>()))
            .Returns(0);
        _imageModuleMock.Setup(p => p.Initialize(It.IsAny<int>()))
            .Returns(1);

        // Act
        Action action = () => _sut.InitializeInProcessingThread();

        // Assert
        action.Should().Throw<PdfModuleInitializationException>();
    }

    [Fact]
    public void InitializeInProcessingThreadShouldThrowExceptionWhenImageInitializationFailed()
    {
        // Arrange
        _libraryLoaderMock.Setup(l => l.Load());

        _pdfModuleMock.Setup(p => p.Initialize(It.IsAny<int>()))
            .Returns(1);
        _imageModuleMock.Setup(p => p.Initialize(It.IsAny<int>()))
            .Returns(0);

        // Act
        Action action = () => _sut.InitializeInProcessingThread();

        // Assert
        action.Should().Throw<ImageModuleInitializationException>();
    }
}
