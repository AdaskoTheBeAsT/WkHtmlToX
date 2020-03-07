using AdaskoTheBeAsT.WkHtmlToX.Settings;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Settings
{
    public sealed class HeaderSettingsTest
    {
        private readonly Fixture _fixture;

        public HeaderSettingsTest()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void ShouldBeProperlyInitialized()
        {
            // Arrange
            var sut = new HeaderSettings();

            // Assert
            using (new AssertionScope())
            {
                sut.Center.Should().BeNull();
            }
        }
    }
}
