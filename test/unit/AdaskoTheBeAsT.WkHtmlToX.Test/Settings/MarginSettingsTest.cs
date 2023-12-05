using AdaskoTheBeAsT.WkHtmlToX.Settings;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Settings;

public sealed class MarginSettingsTest
{
    private readonly Fixture _fixture;

    public MarginSettingsTest()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void ShouldHaveDefaultValuesAfterInvokingEmptyConstructor()
    {
        // Act
        var sut = new MarginSettings();

        // Assert
        using (new AssertionScope())
        {
            sut.Unit.Should().Be(Unit.Millimeters);
            sut.Top.Should().BeNull();
            sut.Right.Should().BeNull();
            sut.Bottom.Should().BeNull();
            sut.Left.Should().BeNull();
        }
    }

    [Fact]
    public void ShouldHaveProperValuesAfterInvokingConstructorWithParameters()
    {
        // Arrange
        var top = _fixture.Create<double>();
        var right = _fixture.Create<double>();
        var bottom = _fixture.Create<double>();
        var left = _fixture.Create<double>();

        // Act
        var sut = new MarginSettings(top, right, bottom, left);

        // Assert
        using (new AssertionScope())
        {
            sut.Unit.Should().Be(Unit.Millimeters);
            sut.Top.Should().Be(top);
            sut.Right.Should().Be(right);
            sut.Bottom.Should().Be(bottom);
            sut.Left.Should().Be(left);
        }
    }

    [Theory]
    [InlineData(Unit.Millimeters, null, null)]
    [InlineData(Unit.Millimeters, 1.0, "1mm")]
    [InlineData(Unit.Millimeters, 1.234, "1.23mm")]
    [InlineData(Unit.Millimeters, 1.236, "1.24mm")]
    [InlineData(Unit.Inches, 1.0, "1in")]
    [InlineData(Unit.Inches, 1.234, "1.23in")]
    [InlineData(Unit.Inches, 1.236, "1.24in")]
    [InlineData(Unit.Centimeters, 1.0, "1cm")]
    [InlineData(Unit.Centimeters, 1.234, "1.23cm")]
    [InlineData(Unit.Centimeters, 1.236, "1.24cm")]
    public void ShouldReturnProperStringWhenInvokingGetMarginValue(
        Unit unit,
        double? value,
        string? expected)
    {
        // Arrange
        var sut = new MarginSettings
        {
            Unit = unit,
        };

        // Act
        var result = sut.GetMarginValue(value);

        // Assert
        result.Should().Be(expected);
    }
}
