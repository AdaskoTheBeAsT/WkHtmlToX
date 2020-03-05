using AdaskoTheBeAsT.WkHtmlToX.Documents;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Documents
{
    public sealed class HtmlToPdfDocumentTest
    {
        [Fact]
        public void ShouldHaveNonNullSettingsAfterInitialization()
        {
            // Arrange
            var sut = new HtmlToPdfDocument();

            // Assert
            using (new AssertionScope())
            {
                sut.GlobalSettings.Should().NotBeNull();
                sut.ObjectSettings.Should().NotBeNull();
            }
        }
    }
}
