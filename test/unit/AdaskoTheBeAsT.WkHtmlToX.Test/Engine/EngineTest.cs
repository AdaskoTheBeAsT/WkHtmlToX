using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.Interop.Execution;
using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.WorkItems;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Moq;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Engine;

public sealed class EngineTest
    : IDisposable
{
    private readonly Mock<IExecutionWorker<WkHtmlToXSession>> _workerMock;
    private readonly WkHtmlToXEngine _sut;

    public EngineTest()
    {
        _workerMock = new Mock<IExecutionWorker<WkHtmlToXSession>>(MockBehavior.Strict);
        _sut = new WkHtmlToXEngine(_workerMock.Object, ownsWorker: false);
    }

    public void Dispose() => _sut.Dispose();

    [Fact]
    public void ShouldThrowExceptionWhenNullConfigurationPassed()
    {
        // Arrange
        Action action = () =>
        {
            using var engine = new WkHtmlToXEngine(configuration: null!);
        };

        // Act and Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ShouldThrowExceptionWhenNullWorkerPassed()
    {
        // Arrange
        Action action = () =>
        {
            using var engine = new WkHtmlToXEngine(worker: null!);
        };

        // Act and Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddConvertWorkItemShouldThrowWhenNullItemPassed()
    {
        // Arrange
        Action action = () => _sut.AddConvertWorkItem(item: null!, CancellationToken.None);

        // Act and Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void InitializeShouldDelegateToWorker()
    {
        // Arrange
        _workerMock.Setup(w => w.Initialize());

        // Act
        _sut.Initialize();

        // Assert
        _workerMock.Verify(w => w.Initialize(), Times.Once);
    }

    [Fact]
    public async Task AddPdfConvertWorkItemShouldForwardSuccessResultAsync()
    {
        // Arrange
        _workerMock
            .Setup(w => w.ExecuteAsync(
                It.IsAny<Func<WkHtmlToXSession, CancellationToken, bool>>(),
                It.IsAny<ExecutionRequestOptions?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(true));

        var item = new PdfConvertWorkItem(new HtmlToPdfDocument(), _ => Stream.Null);

        // Act
        _sut.AddConvertWorkItem(item, CancellationToken.None);
#pragma warning disable VSTHRD003
        var result = await item.TaskCompletionSource.Task;
#pragma warning restore VSTHRD003

        // Assert
        using (new AssertionScope())
        {
            result.Should().BeTrue();
            _workerMock.Verify(
                w => w.ExecuteAsync(
                    It.IsAny<Func<WkHtmlToXSession, CancellationToken, bool>>(),
                    It.IsAny<ExecutionRequestOptions?>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }

    [Fact]
    public async Task AddImageConvertWorkItemShouldForwardSuccessResultAsync()
    {
        // Arrange
        _workerMock
            .Setup(w => w.ExecuteAsync(
                It.IsAny<Func<WkHtmlToXSession, CancellationToken, bool>>(),
                It.IsAny<ExecutionRequestOptions?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(true));

        var item = new ImageConvertWorkItem(new HtmlToImageDocument(), _ => Stream.Null);

        // Act
        _sut.AddConvertWorkItem(item, CancellationToken.None);
#pragma warning disable VSTHRD003
        var result = await item.TaskCompletionSource.Task;
#pragma warning restore VSTHRD003

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public Task AddConvertWorkItemShouldForwardExceptionAsync()
    {
        // Arrange
        _workerMock
            .Setup(w => w.ExecuteAsync(
                It.IsAny<Func<WkHtmlToXSession, CancellationToken, bool>>(),
                It.IsAny<ExecutionRequestOptions?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.FromException<bool>(new InvalidOperationException("boom")));

        var item = new PdfConvertWorkItem(new HtmlToPdfDocument(), _ => Stream.Null);

        // Act
        _sut.AddConvertWorkItem(item, CancellationToken.None);
#pragma warning disable VSTHRD003
        Func<Task<bool>> action = async () => await item.TaskCompletionSource.Task;
#pragma warning restore VSTHRD003

        // Assert
        return action.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public Task AddConvertWorkItemShouldForwardCancellationAsync()
    {
        // Arrange
        var tcs = new TaskCompletionSource<bool>();
#pragma warning disable xUnit1051
        tcs.TrySetCanceled();
#pragma warning restore xUnit1051
        _workerMock
            .Setup(w => w.ExecuteAsync(
                It.IsAny<Func<WkHtmlToXSession, CancellationToken, bool>>(),
                It.IsAny<ExecutionRequestOptions?>(),
                It.IsAny<CancellationToken>()))
            .Returns(tcs.Task);

        var item = new PdfConvertWorkItem(new HtmlToPdfDocument(), _ => Stream.Null);

        // Act
#pragma warning disable xUnit1051
        _sut.AddConvertWorkItem(item, CancellationToken.None);
#pragma warning restore xUnit1051
#pragma warning disable VSTHRD003
        Func<Task<bool>> action = async () => await item.TaskCompletionSource.Task;
#pragma warning restore VSTHRD003

        // Assert
        return action.Should().ThrowAsync<TaskCanceledException>();
    }

    [Fact]
    public void DisposeShouldNotDisposeWorkerWhenOwnershipIsFalse()
    {
        // Arrange
        var mock = new Mock<IExecutionWorker<WkHtmlToXSession>>(MockBehavior.Strict);
#pragma warning disable IDISP017 // explicit Dispose verification
        var engine = new WkHtmlToXEngine(mock.Object, ownsWorker: false);

        // Act
        engine.Dispose();
#pragma warning restore IDISP017

        // Assert
        mock.Verify(w => w.Dispose(), Times.Never);
    }

    [Fact]
    public void DisposeShouldDisposeWorkerWhenOwnershipIsTrue()
    {
        // Arrange
        var mock = new Mock<IExecutionWorker<WkHtmlToXSession>>(MockBehavior.Strict);
        mock.Setup(w => w.Dispose());
#pragma warning disable IDISP017 // explicit Dispose verification
        var engine = new WkHtmlToXEngine(mock.Object, ownsWorker: true);

        // Act
        engine.Dispose();
#pragma warning restore IDISP017

        // Assert
        mock.Verify(w => w.Dispose(), Times.Once);
    }
}
