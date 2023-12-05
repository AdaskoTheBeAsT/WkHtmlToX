using AdaskoTheBeAsT.WkHtmlToX.Settings;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Settings;

public sealed class ImageSettingsTest
{
    private readonly Fixture _fixture;

    public ImageSettingsTest()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void ShouldBeProperlyInitialized()
    {
        // Act
        var sut = new ImageSettings();

        // Assert
        using (new AssertionScope())
        {
            sut.LoadSettings.Should().NotBeNull();
            sut.WebSettings.Should().NotBeNull();
            sut.CookieJar.Should().BeNull();
            sut.CropHeight.Should().BeNull();
            sut.CropLeft.Should().BeNull();
            sut.CropTop.Should().BeNull();
            sut.CropWidth.Should().BeNull();
            sut.Format.Should().BeNull();
            sut.In.Should().BeNull();
            sut.Out.Should().BeNull();
            sut.Quality.Should().BeNull();
            sut.ScreenWidth.Should().BeNull();
            sut.SmartWidth.Should().BeNull();
            sut.Transparent.Should().BeNull();
        }
    }

    [Fact]
    public void ShouldAllowToSetValues()
    {
        // Arrange
        var loadSettings = new LoadSettings();
        var webSettings = new WebSettings();
        var cookieJar = _fixture.Create<string>();
        var cropHeight = _fixture.Create<string>();
        var cropLeft = _fixture.Create<string>();
        var cropTop = _fixture.Create<string>();
        var cropWidth = _fixture.Create<string>();
        var format = _fixture.Create<string>();
        var pin = _fixture.Create<string>();
        var pout = _fixture.Create<string>();
        var quality = _fixture.Create<string>();
        var screenWidth = _fixture.Create<string>();
        var smartWidth = _fixture.Create<bool>();
        var transparent = _fixture.Create<bool>();

        // Act
        var sut = new ImageSettings
        {
            LoadSettings = loadSettings,
            WebSettings = webSettings,
            CookieJar = cookieJar,
            CropHeight = cropHeight,
            CropLeft = cropLeft,
            CropTop = cropTop,
            CropWidth = cropWidth,
            Format = format,
            In = pin,
            Out = pout,
            Quality = quality,
            ScreenWidth = screenWidth,
            SmartWidth = smartWidth,
            Transparent = transparent,
        };

        // Assert
        using (new AssertionScope())
        {
            sut.LoadSettings.Should().Be(loadSettings);
            sut.WebSettings.Should().Be(webSettings);
            sut.CookieJar.Should().Be(cookieJar);
            sut.CropHeight.Should().Be(cropHeight);
            sut.CropLeft.Should().Be(cropLeft);
            sut.CropTop.Should().Be(cropTop);
            sut.CropWidth.Should().Be(cropWidth);
            sut.Format.Should().Be(format);
            sut.In.Should().Be(pin);
            sut.Out.Should().Be(pout);
            sut.Quality.Should().Be(quality);
            sut.ScreenWidth.Should().Be(screenWidth);
            sut.SmartWidth.Should().Be(smartWidth);
            sut.Transparent.Should().Be(transparent);
        }
    }
}
