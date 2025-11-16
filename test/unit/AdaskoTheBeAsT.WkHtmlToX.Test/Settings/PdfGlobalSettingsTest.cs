using AdaskoTheBeAsT.WkHtmlToX.Settings;
using AutoFixture;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Settings;

public sealed class PdfGlobalSettingsTest
{
    private readonly Fixture _fixture;

    public PdfGlobalSettingsTest()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void ShouldBeProperlyInitialized()
    {
        // Act
        var sut = new PdfGlobalSettings();

        // Assert
        using (new AssertionScope())
        {
            sut.Margins.Should().NotBeNull();
            sut.Collate.Should().BeNull();
            sut.ColorMode.Should().BeNull();
            sut.CookieJar.Should().BeNull();
            sut.Copies.Should().BeNull();
            sut.DPI.Should().BeNull();
            sut.DocumentTitle.Should().BeNull();
            sut.DumpOutline.Should().BeNull();
            sut.ImageDPI.Should().BeNull();
            sut.ImageQuality.Should().BeNull();
            sut.Orientation.Should().BeNull();
            sut.Out.Should().BeNull();
            sut.Outline.Should().BeNull();
            sut.OutlineDepth.Should().BeNull();
            sut.PageOffset.Should().BeNull();
            sut.PaperSize.Should().BeNull();
            sut.UseCompression.Should().BeNull();
            sut.PaperWidth.Should().BeNull();
            sut.PaperHeight.Should().BeNull();
            sut.MarginTop.Should().BeNull();
            sut.MarginRight.Should().BeNull();
            sut.MarginBottom.Should().BeNull();
            sut.MarginLeft.Should().BeNull();
        }
    }

#pragma warning disable MA0051 // Method is too long
    [Fact]
    public void ShouldAllowToSetValues()
    {
        // Arrange
        var marginSettings = _fixture.Create<MarginSettings>();
        var collate = _fixture.Create<bool>();
        var colorMode = _fixture.Create<ColorMode>();
        var cookieJar = _fixture.Create<string>();
        var copies = _fixture.Create<int>();
        var dpi = _fixture.Create<int>();
        var documentTitle = _fixture.Create<string>();
        var dumpOutline = _fixture.Create<string>();
        var imageDpi = _fixture.Create<int>();
        var imageQuality = _fixture.Create<int>();
        var orientation = _fixture.Create<Orientation>();
        var pout = _fixture.Create<string>();
        var outline = _fixture.Create<bool>();
        var outlineDepth = _fixture.Create<int>();
        var pageOffset = _fixture.Create<int>();
        var paperSize = _fixture.Create<PechkinPaperSize>();
        var useCompression = _fixture.Create<bool>();

        // Act
        var sut = new PdfGlobalSettings
        {
            Margins = marginSettings,
            Collate = collate,
            ColorMode = colorMode,
            CookieJar = cookieJar,
            Copies = copies,
            DPI = dpi,
            DocumentTitle = documentTitle,
            DumpOutline = dumpOutline,
            ImageDPI = imageDpi,
            ImageQuality = imageQuality,
            Orientation = orientation,
            Out = pout,
            Outline = outline,
            OutlineDepth = outlineDepth,
            PageOffset = pageOffset,
            PaperSize = paperSize,
            UseCompression = useCompression,
        };

        // Assert
        using (new AssertionScope())
        {
            sut.Margins.Should().Be(marginSettings);
            sut.Collate.Should().Be(collate);
            sut.ColorMode.Should().Be(colorMode);
            sut.CookieJar.Should().Be(cookieJar);
            sut.Copies.Should().Be(copies);
            sut.DPI.Should().Be(dpi);
            sut.DocumentTitle.Should().Be(documentTitle);
            sut.DumpOutline.Should().Be(dumpOutline);
            sut.ImageDPI.Should().Be(imageDpi);
            sut.ImageQuality.Should().Be(imageQuality);
            sut.Orientation.Should().Be(orientation);
            sut.Out.Should().Be(pout);
            sut.Outline.Should().Be(outline);
            sut.OutlineDepth.Should().Be(outlineDepth);
            sut.PageOffset.Should().Be(pageOffset);
            sut.PaperSize.Should().Be(paperSize);
            sut.UseCompression.Should().Be(useCompression);
            sut.PaperWidth.Should().Be(paperSize.Width);
            sut.PaperHeight.Should().Be(paperSize.Height);
            sut.MarginTop.Should().Be(sut.Margins.GetMarginValue(marginSettings.Top));
            sut.MarginRight.Should().Be(sut.Margins.GetMarginValue(marginSettings.Right));
            sut.MarginBottom.Should().Be(sut.Margins.GetMarginValue(marginSettings.Bottom));
            sut.MarginLeft.Should().Be(sut.Margins.GetMarginValue(marginSettings.Left));
        }
    }
#pragma warning restore MA0051 // Method is too long
}
