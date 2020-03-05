using System;
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
            Action action = () => _ = new PdfConvertWorkItem(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // Act & Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ShouldHaveNonNullProperties()
        {
            // Arrange
            var documentMock = _mockRepository.Create<IHtmlToPdfDocument>().Object;

            // Act
            var sut = new PdfConvertWorkItem(documentMock);

            // Assert
            using (new AssertionScope())
            {
                sut.Document.Should().NotBeNull();
                sut.Document.Should().Be(documentMock);
                sut.TaskCompletionSource.Should().NotBeNull();
            }
        }
    }
}
