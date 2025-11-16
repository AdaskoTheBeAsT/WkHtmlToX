using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Documents;

public sealed class HtmlToImageDocumentTest
{
    [Fact]
    public void ShouldHaveNonNullImageSettingsAfterInitialization()
    {
        // Arrange
        var sut = new HtmlToImageDocument();

        // Assert
        sut.ImageSettings.Should().NotBeNull();
    }
}
