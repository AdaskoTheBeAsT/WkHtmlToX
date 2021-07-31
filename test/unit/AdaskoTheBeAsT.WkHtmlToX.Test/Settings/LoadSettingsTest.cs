using System.Collections.Generic;
using AdaskoTheBeAsT.WkHtmlToX.Settings;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Settings
{
    public sealed class LoadSettingsTest
    {
        private readonly Fixture _fixture;

        public LoadSettingsTest()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void ShouldBeProperlyInitialized()
        {
            // Act
            var sut = new LoadSettings();

            // Assert
            using (new AssertionScope())
            {
                sut.BlockLocalFileAccess.Should().BeNull();
                sut.Cookies.Should().BeNull();
                sut.CustomHeaders.Should().BeNull();
                sut.DebugJavascript.Should().BeNull();
                sut.JSDelay.Should().BeNull();
                sut.LoadErrorHandling.Should().BeNull();
                sut.Password.Should().BeNull();
                sut.Proxy.Should().BeNull();
                sut.RepeatCustomHeaders.Should().BeNull();
                sut.StopSlowScript.Should().BeNull();
                sut.Username.Should().BeNull();
                sut.ZoomFactor.Should().BeNull();
            }
        }

        [Fact]
        public void ShouldAllowToSetValues()
        {
            // Arrange
            var blockLocalFileAccess = _fixture.Create<bool>();
            var cookies = _fixture.Create<Dictionary<string, string>>();
            var debugJavascript = _fixture.Create<bool>();
            var customHeaders = _fixture.Create<Dictionary<string, string>>();
            var jasvaScriptDelay = _fixture.Create<int>();
            var loadErrorHandling = _fixture.Create<ContentErrorHandling>();
            var password = _fixture.Create<string>();
            var proxy = _fixture.Create<string>();
            var repeatCustomHeaders = _fixture.Create<bool>();
            var stopSlowScript = _fixture.Create<bool>();
            var username = _fixture.Create<string>();
            var zoomFactor = _fixture.Create<double>();

            // Act
            var sut = new LoadSettings
            {
                BlockLocalFileAccess = blockLocalFileAccess,
                Cookies = cookies,
                DebugJavascript = debugJavascript,
                CustomHeaders = customHeaders,
                JSDelay = jasvaScriptDelay,
                LoadErrorHandling = loadErrorHandling,
                Password = password,
                Proxy = proxy,
                RepeatCustomHeaders = repeatCustomHeaders,
                StopSlowScript = stopSlowScript,
                Username = username,
                ZoomFactor = zoomFactor,
            };

            // Assert
            using (new AssertionScope())
            {
                sut.BlockLocalFileAccess.Should().Be(blockLocalFileAccess);
                sut.Cookies.Should().BeEquivalentTo(cookies);
                sut.CustomHeaders.Should().BeEquivalentTo(customHeaders);
                sut.DebugJavascript.Should().Be(debugJavascript);
                sut.JSDelay.Should().Be(jasvaScriptDelay);
                sut.LoadErrorHandling.Should().Be(loadErrorHandling);
                sut.Password.Should().Be(password);
                sut.Proxy.Should().Be(proxy);
                sut.RepeatCustomHeaders.Should().Be(repeatCustomHeaders);
                sut.StopSlowScript.Should().Be(stopSlowScript);
                sut.Username.Should().Be(username);
                sut.ZoomFactor.Should().Be(zoomFactor);
            }
        }
    }
}
