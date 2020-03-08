using System;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
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
        public void ShouldThrowExceptionWhenNullPassedInConstructor()
        {
            // Assert
            var pdfModule = _fixture.Create<IWkHtmlToPdfModuleFactory>();

            // Arrange
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action action = () => _ = new BasicPdfConverter(null, pdfModule);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Act & Assert
            action.Should().Throw<ArgumentNullException>();
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
    }
}
