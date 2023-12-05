using System.Text;
using AdaskoTheBeAsT.WkHtmlToX.Settings;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Settings;

public sealed class PdfObjectSettingsTest
{
    private readonly Fixture _fixture;

    public PdfObjectSettingsTest()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void ShouldBeProperlyInitialized()
    {
        // Act
        var sut = new PdfObjectSettings();

        // Assert
        using (new AssertionScope())
        {
            sut.LoadSettings.Should().NotBeNull();
            sut.FooterSettings.Should().NotBeNull();
            sut.HeaderSettings.Should().NotBeNull();
            sut.WebSettings.Should().NotBeNull();
            sut.BackLinks.Should().BeNull();
            sut.CaptionText.Should().BeNull();
            sut.Encoding.Should().BeNull();
            sut.FontScale.Should().BeNull();
            sut.ForwardLinks.Should().BeNull();
            sut.HtmlContent.Should().BeNull();
            sut.IncludeInOutline.Should().BeNull();
            sut.Indentation.Should().BeNull();
            sut.Page.Should().BeNull();
            sut.PagesCount.Should().BeNull();
            sut.ProduceForms.Should().BeNull();
            sut.UseDottedLines.Should().BeNull();
            sut.UseExternalLinks.Should().BeNull();
            sut.UseLocalLinks.Should().BeNull();
            sut.Xsl.Should().BeNull();
        }
    }

#pragma warning disable MA0051 // Method is too long
    [Fact]
    public void ShouldAllowToSetValues()
    {
        // Arrange
        var backLinks = _fixture.Create<bool>();
        var captionText = _fixture.Create<string>();
        var encoding = _fixture.Create<Encoding>();
        var fontScale = _fixture.Create<string>();
        var footerSettings = _fixture.Create<SectionSettings>();
        var forwardLinks = _fixture.Create<bool>();
        var headerSettings = _fixture.Create<SectionSettings>();
        var htmlContent = _fixture.Create<string>();
        var includeInOutline = _fixture.Create<bool>();
        var indentation = _fixture.Create<string>();
        var loadSettings = _fixture.Create<LoadSettings>();
        var page = _fixture.Create<string>();
        var pagesCount = _fixture.Create<bool>();
        var produceForms = _fixture.Create<bool>();
        var useDottedLines = _fixture.Create<bool>();
        var useExternalLinks = _fixture.Create<bool>();
        var useLocalLinks = _fixture.Create<bool>();
        var webSettings = _fixture.Create<WebSettings>();
        var xsl = _fixture.Create<string>();

        // Act
        var sut = new PdfObjectSettings
        {
            BackLinks = backLinks,
            CaptionText = captionText,
            Encoding = encoding,
            FontScale = fontScale,
            FooterSettings = footerSettings,
            ForwardLinks = forwardLinks,
            HeaderSettings = headerSettings,
            HtmlContent = htmlContent,
            IncludeInOutline = includeInOutline,
            Indentation = indentation,
            LoadSettings = loadSettings,
            Page = page,
            PagesCount = pagesCount,
            ProduceForms = produceForms,
            UseDottedLines = useDottedLines,
            UseExternalLinks = useExternalLinks,
            UseLocalLinks = useLocalLinks,
            WebSettings = webSettings,
            Xsl = xsl,
        };

        // Assert
        using (new AssertionScope())
        {
            sut.BackLinks.Should().Be(backLinks);
            sut.CaptionText.Should().Be(captionText);
            sut.Encoding.Should().Be(encoding);
            sut.FontScale.Should().Be(fontScale);
            sut.FooterSettings.Should().Be(footerSettings);
            sut.ForwardLinks.Should().Be(forwardLinks);
            sut.HeaderSettings.Should().Be(headerSettings);
            sut.HtmlContent.Should().Be(htmlContent);
            sut.IncludeInOutline.Should().Be(includeInOutline);
            sut.Indentation.Should().Be(indentation);
            sut.LoadSettings.Should().Be(loadSettings);
            sut.Page.Should().Be(page);
            sut.PagesCount.Should().Be(pagesCount);
            sut.ProduceForms.Should().Be(produceForms);
            sut.UseDottedLines.Should().Be(useDottedLines);
            sut.UseExternalLinks.Should().Be(useExternalLinks);
            sut.UseLocalLinks.Should().Be(useLocalLinks);
            sut.WebSettings.Should().Be(webSettings);
            sut.Xsl.Should().Be(xsl);
        }
    }
#pragma warning restore MA0051 // Method is too long
}
