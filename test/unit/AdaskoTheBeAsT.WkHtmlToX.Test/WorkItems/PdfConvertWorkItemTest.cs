using System;
using System.IO;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.WorkItems;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.WorkItems
{
    public sealed class PdfConvertWorkItemTest
        : IDisposable
    {
        private readonly Fixture _fixture;
        private readonly MockRepository _mockRepository;

        public PdfConvertWorkItemTest()
        {
            _fixture = new Fixture();
            _mockRepository = new MockRepository(MockBehavior.Strict);
        }

        public void Dispose()
        {
            _mockRepository.VerifyAll();
        }

        [Fact]
        public void ShouldThrowArgumentNullExceptionWhenNullPassedToConstructor()
        {
            // Arrange
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8603 // Possible null reference return.
            Action action = () => _ = new PdfConvertWorkItem(null, length => Stream.Null);
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Act & Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ShouldHaveNonNullProperties()
        {
            // Arrange
            var documentMock = _mockRepository.Create<IHtmlToPdfDocument>().Object;
            var stream = Stream.Null;

            // Act
            var sut = new PdfConvertWorkItem(documentMock, length => stream);

            // Assert
            using (new AssertionScope())
            {
                sut.Document.Should().NotBeNull();
                sut.Document.Should().Be(documentMock);
                sut.StreamFunc.Should().NotBeNull();
                sut.StreamFunc(0).Should().Be(stream);
                sut.TaskCompletionSource.Should().NotBeNull();
            }
        }
    }
}
