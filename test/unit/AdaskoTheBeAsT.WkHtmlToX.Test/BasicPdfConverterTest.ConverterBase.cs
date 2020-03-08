using System;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.EventDefinitions;
using AdaskoTheBeAsT.WkHtmlToX.Utils;
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
        public void ShouldThrowExceptionWhenNullPassedInModuleConstructor()
        {
            // Arrange
            var pdfModuleMock = new Mock<IWkHtmlToPdfModuleFactory>();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action action = () => _ = new BasicPdfConverter(null, pdfModuleMock.Object);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Act & Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ShouldAllowToObtainProcessingDocument()
        {
            // Arrange
            var doc = new Mock<IDocument>().Object;

            // Act
            _sut.ProcessingDocument = doc;

            // Assert
            _sut.ProcessingDocument.Should().Be(doc);
        }

        [Fact]
        public void RegisterEventsShouldThrowExceptionWhenPointerZeroPassed()
        {
            // Arrange
            Action action = () => _sut.RegisterEvents(IntPtr.Zero);

            // Act & Assert
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void RegisterEventsShouldNotRegisterWhereEventsNotAttached()
        {
            // Arrange
            _module.Setup(m => m.SetErrorCallback(It.IsAny<IntPtr>(), It.IsAny<StringCallback>()));
            _module.Setup(m => m.SetWarningCallback(It.IsAny<IntPtr>(), It.IsAny<StringCallback>()));
            _module.Setup(m => m.SetFinishedCallback(It.IsAny<IntPtr>(), It.IsAny<IntCallback>()));
            _module.Setup(m => m.SetPhaseChangedCallback(It.IsAny<IntPtr>(), It.IsAny<VoidCallback>()));
            _module.Setup(m => m.SetProgressChangedCallback(It.IsAny<IntPtr>(), It.IsAny<VoidCallback>()));

            // Act
            _sut.RegisterEvents(new IntPtr(12));

            // Assert
            using (new AssertionScope())
            {
                _module.Verify(
                    m =>
                        m.SetErrorCallback(It.IsAny<IntPtr>(), It.IsAny<StringCallback>()), Times.Never);
                _module.Verify(
                    m =>
                        m.SetWarningCallback(It.IsAny<IntPtr>(), It.IsAny<StringCallback>()), Times.Never);
                _module.Verify(
                    m =>
                        m.SetFinishedCallback(It.IsAny<IntPtr>(), It.IsAny<IntCallback>()), Times.Never);
                _module.Verify(
                    m =>
                        m.SetPhaseChangedCallback(It.IsAny<IntPtr>(), It.IsAny<VoidCallback>()), Times.Never);
                _module.Verify(
                    m =>
                        m.SetProgressChangedCallback(It.IsAny<IntPtr>(), It.IsAny<VoidCallback>()), Times.Never);
            }
        }

        [Fact]
        public void RegisterEventsShouldRegisterErrorCallbackWhenSpecified()
        {
            // Arrange
            _module.Setup(m => m.SetErrorCallback(It.IsAny<IntPtr>(), It.IsAny<StringCallback>()));
            _sut.Error += (
                sender,
                args) =>
            {
            };

            // Act
            _sut.RegisterEvents(new IntPtr(12));

            // Assert
            using (new AssertionScope())
            {
                _module.Verify(
                    m =>
                        m.SetErrorCallback(It.IsAny<IntPtr>(), It.IsAny<StringCallback>()), Times.Once);
            }
        }

        [Fact]
        public void RegisterEventsShouldRegisterWarningCallbackWhenSpecified()
        {
            // Arrange
            _module.Setup(m => m.SetWarningCallback(It.IsAny<IntPtr>(), It.IsAny<StringCallback>()));
            _sut.Warning += (
                sender,
                args) =>
            {
            };

            // Act
            _sut.RegisterEvents(new IntPtr(12));

            // Assert
            using (new AssertionScope())
            {
                _module.Verify(
                    m =>
                        m.SetWarningCallback(It.IsAny<IntPtr>(), It.IsAny<StringCallback>()), Times.Once);
            }
        }

        [Fact]
        public void RegisterEventsShouldRegisterFinishedCallbackWhenSpecified()
        {
            // Arrange
            _module.Setup(m => m.SetFinishedCallback(It.IsAny<IntPtr>(), It.IsAny<IntCallback>()));
            _sut.Finished += (
                sender,
                args) =>
            {
            };

            // Act
            _sut.RegisterEvents(new IntPtr(12));

            // Assert
            using (new AssertionScope())
            {
                _module.Verify(
                    m =>
                        m.SetFinishedCallback(It.IsAny<IntPtr>(), It.IsAny<IntCallback>()), Times.Once);
            }
        }

        [Fact]
        public void RegisterEventsShouldRegisterPhaseChangedCallbackWhenSpecified()
        {
            // Arrange
            _module.Setup(m => m.SetPhaseChangedCallback(It.IsAny<IntPtr>(), It.IsAny<VoidCallback>()));
            _sut.PhaseChanged += (
                sender,
                args) =>
            {
            };

            // Act
            _sut.RegisterEvents(new IntPtr(12));

            // Assert
            using (new AssertionScope())
            {
                _module.Verify(
                    m =>
                        m.SetPhaseChangedCallback(It.IsAny<IntPtr>(), It.IsAny<VoidCallback>()), Times.Once);
            }
        }

        [Fact]
        public void RegisterEventsShouldRegisterProgressChangedCallbackWhenSpecified()
        {
            // Arrange
            _module.Setup(m => m.SetProgressChangedCallback(It.IsAny<IntPtr>(), It.IsAny<VoidCallback>()));
            _sut.ProgressChanged += (
                sender,
                args) =>
            {
            };

            // Act
            _sut.RegisterEvents(new IntPtr(12));

            // Assert
            using (new AssertionScope())
            {
                _module.Verify(
                    m =>
                        m.SetProgressChangedCallback(It.IsAny<IntPtr>(), It.IsAny<VoidCallback>()), Times.Once);
            }
        }

        [Fact]
        public void OnErrorShouldNotThrowWhenNoEvent()
        {
            // Arrange
            var errorMessage = _fixture.Create<string>();
            Action action = () => _sut.OnError(new IntPtr(1), errorMessage);

            // Act & Assert
            action.Should().NotThrow();
        }

        [Fact]
        public void OnErrorShouldPassDetailsAboutError()
        {
            // Arrange
            ErrorEventArgs? result = default;
            var errorMessage = _fixture.Create<string>();
            var doc = new Mock<IDocument>().Object;

            _sut.ProcessingDocument = doc;
            _sut.Error += (
                sender,
                args) =>
            {
                result = args;
            };

            // Act
            _sut.OnError(new IntPtr(1), errorMessage);

            // Assert
            using (new AssertionScope())
            {
                result!.Document.Should().Be(doc);
                result!.Message.Should().Be(errorMessage);
            }
        }

        [Fact]
        public void OnWarningShouldNotThrowWhenNoEvent()
        {
            // Arrange
            var errorMessage = _fixture.Create<string>();
            Action action = () => _sut.OnWarning(new IntPtr(1), errorMessage);

            // Act & Assert
            action.Should().NotThrow();
        }

        [Fact]
        public void OnWarningShouldPassDetailsAboutError()
        {
            // Arrange
            WarningEventArgs? result = default;
            var errorMessage = _fixture.Create<string>();
            var doc = new Mock<IDocument>().Object;

            _sut.ProcessingDocument = doc;
            _sut.Warning += (
                sender,
                args) =>
            {
                result = args;
            };

            // Act
            _sut.OnWarning(new IntPtr(1), errorMessage);

            // Assert
            using (new AssertionScope())
            {
                result!.Document.Should().Be(doc);
                result!.Message.Should().Be(errorMessage);
            }
        }

        [Fact]
        public void OnFinishedShouldNotThrowWhenNoEvent()
        {
            // Arrange
            var code = _fixture.Create<int>();
            Action action = () => _sut.OnFinished(new IntPtr(1), code);

            // Act & Assert
            action.Should().NotThrow();
        }

        [Fact]
        public void OnFinishedShouldPassDetailsAboutError()
        {
            // Arrange
            FinishedEventArgs? result = default;
            var code = _fixture.Create<int>();
            var doc = new Mock<IDocument>().Object;

            _sut.ProcessingDocument = doc;
            _sut.Finished += (
                sender,
                args) =>
            {
                result = args;
            };

            // Act
            _sut.OnFinished(new IntPtr(1), code);

            // Assert
            using (new AssertionScope())
            {
                result!.Document.Should().Be(doc);
                result!.Success.Should().Be(code == 1);
            }
        }

        [Fact]
        public void OnPhaseChangedShouldNotThrowWhenNoEvent()
        {
            // Arrange
            Action action = () => _sut.OnPhaseChanged(new IntPtr(1));

            // Act & Assert
            action.Should().NotThrow();
        }

        [Fact]
        public void OnPhaseChangedShouldPassDetailsAboutError()
        {
            // Arrange
            var phaseCount = _fixture.Create<int>();
            var currentPhase = _fixture.Create<int>();
            var phaseDescription = _fixture.Create<string>();
            _module.Setup(m => m.GetPhaseCount(It.IsAny<IntPtr>()))
                .Returns(phaseCount);
            _module.Setup(m => m.GetCurrentPhase(It.IsAny<IntPtr>()))
                .Returns(currentPhase);
            _module.Setup(m => m.GetPhaseDescription(It.IsAny<IntPtr>(), It.IsAny<int>()))
                .Returns(phaseDescription);

            PhaseChangedEventArgs? result = default;
            var doc = new Mock<IDocument>().Object;

            _sut.ProcessingDocument = doc;
            _sut.PhaseChanged += (
                sender,
                args) =>
            {
                result = args;
            };

            // Act
            _sut.OnPhaseChanged(new IntPtr(1));

            // Assert
            using (new AssertionScope())
            {
                result!.Document.Should().Be(doc);
                result!.PhaseCount.Should().Be(phaseCount);
                result!.CurrentPhase.Should().Be(currentPhase);
                result!.Description.Should().Be(phaseDescription);
            }
        }

        [Fact]
        public void OnProgressChangedShouldNotThrowWhenNoEvent()
        {
            // Arrange
            Action action = () => _sut.OnProgressChanged(new IntPtr(1));

            // Act & Assert
            action.Should().NotThrow();
        }

        [Fact]
        public void OnProgressChangedShouldPassDetailsAboutError()
        {
            // Arrange
            var progressDescription = _fixture.Create<string>();
            _module.Setup(m => m.GetProgressString(It.IsAny<IntPtr>()))
                .Returns(progressDescription);

            ProgressChangedEventArgs? result = default;
            var doc = new Mock<IDocument>().Object;

            _sut.ProcessingDocument = doc;
            _sut.ProgressChanged += (
                sender,
                args) =>
            {
                result = args;
            };

            // Act
            _sut.OnProgressChanged(new IntPtr(1));

            // Assert
            using (new AssertionScope())
            {
                result!.Document.Should().Be(doc);
                result!.Description.Should().Be(progressDescription);
            }
        }
    }
}
