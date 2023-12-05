using AdaskoTheBeAsT.WkHtmlToX.Settings;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Settings;

public sealed class SectionSettingsTest
{
    private readonly Fixture _fixture;

    public SectionSettingsTest()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void ShouldBeProperlyInitialized()
    {
        // Arrange
        var sut = new SectionSettings();

        // Assert
        using (new AssertionScope())
        {
            sut.Center.Should().BeNull();
            sut.FontName.Should().BeNull();
            sut.FontSize.Should().BeNull();
            sut.HtmlUrl.Should().BeNull();
            sut.Left.Should().BeNull();
            sut.Line.Should().BeNull();
            sut.Right.Should().BeNull();
            sut.Spacing.Should().BeNull();
        }
    }

    [Fact]
    public void ShouldAllowToSetValues()
    {
        // Arrange
        var center = _fixture.Create<string>();
        var fontName = _fixture.Create<string>();
        var fontSize = _fixture.Create<int>();
        var htmlUrl = _fixture.Create<string>();
        var left = _fixture.Create<string>();
        var line = _fixture.Create<bool>();
        var right = _fixture.Create<string>();
        var spacing = _fixture.Create<double>();

        // Act
        var sut = new SectionSettings
        {
            Center = center,
            FontName = fontName,
            FontSize = fontSize,
            HtmlUrl = htmlUrl,
            Left = left,
            Line = line,
            Right = right,
            Spacing = spacing,
        };

        // Assert
        using (new AssertionScope())
        {
            sut.Center.Should().Be(center);
            sut.FontName.Should().Be(fontName);
            sut.FontSize.Should().Be(fontSize);
            sut.HtmlUrl.Should().Be(htmlUrl);
            sut.Left.Should().Be(left);
            sut.Line.Should().Be(line);
            sut.Right.Should().Be(right);
            sut.Spacing.Should().Be(spacing);
        }
    }
}
