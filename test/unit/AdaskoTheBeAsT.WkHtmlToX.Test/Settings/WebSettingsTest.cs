using AdaskoTheBeAsT.WkHtmlToX.Settings;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Settings
{
    public sealed class WebSettingsTest
    {
        private readonly Fixture _fixture;

        public WebSettingsTest()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void ShouldBeProperlyInitialized()
        {
            // Arrange
            var sut = new WebSettings();

            // Assert
            using (new AssertionScope())
            {
                sut.Background.Should().BeNull();
                sut.DefaultEncoding.Should().BeNull();
                sut.EnableIntelligentShrinking.Should().BeNull();
                sut.EnableJavascript.Should().BeNull();
                sut.EnablePlugins.Should().BeNull();
                sut.LoadImages.Should().BeNull();
                sut.MinimumFontSize.Should().BeNull();
                sut.PrintMediaType.Should().BeNull();
                sut.UserStyleSheet.Should().BeNull();
            }
        }

        [Fact]
        public void ShouldAllowToSetValues()
        {
            // Arrange
            var background = _fixture.Create<bool>();
            var defaultEncoding = _fixture.Create<string>();
            var enableIntelligentShrinking = _fixture.Create<bool>();
            var enableJavascript = _fixture.Create<bool>();
            var enablePlugins = _fixture.Create<bool>();
            var loadImages = _fixture.Create<bool>();
            var minimumFontSize = _fixture.Create<int>();
            var printMediaType = _fixture.Create<bool>();
            var userStyleSheet = _fixture.Create<string>();

            // Act
            var sut = new WebSettings
            {
                Background = background,
                DefaultEncoding = defaultEncoding,
                EnableIntelligentShrinking = enableIntelligentShrinking,
                EnableJavascript = enableJavascript,
                EnablePlugins = enablePlugins,
                LoadImages = loadImages,
                MinimumFontSize = minimumFontSize,
                PrintMediaType = printMediaType,
                UserStyleSheet = userStyleSheet,
            };

            // Assert
            using (new AssertionScope())
            {
                sut.Background.Should().Be(background);
                sut.DefaultEncoding.Should().Be(defaultEncoding);
                sut.EnableIntelligentShrinking.Should().Be(enableIntelligentShrinking);
                sut.EnableJavascript.Should().Be(enableJavascript);
                sut.EnablePlugins.Should().Be(enablePlugins);
                sut.LoadImages.Should().Be(loadImages);
                sut.MinimumFontSize.Should().Be(minimumFontSize);
                sut.PrintMediaType.Should().Be(printMediaType);
                sut.UserStyleSheet.Should().Be(userStyleSheet);
            }
        }
    }
}
