using System;
using AdaskoTheBeAsT.WkHtmlToX.Utils;
using AutoFixture;
using FluentAssertions;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Utils;

public sealed class WkHtmlAttributeTest
{
    private readonly Fixture _fixture;

    public WkHtmlAttributeTest()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void ShouldThrowArgumentNUllExceptionWhenNullPassedIntoAttribute()
    {
        // Arrange
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

        // ReSharper disable once AssignmentIsFullyDiscarded
        Action action = () => _ = new WkHtmlAttribute(name: null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Act and Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ShouldThrowArgumentNUllExceptionWhenEmptyPassedIntoAttribute()
    {
        // Arrange
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

        // ReSharper disable once AssignmentIsFullyDiscarded
        Action action = () => _ = new WkHtmlAttribute(string.Empty);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Act and Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ShouldHaveSameNameAsPassedIntoConstructor()
    {
        // Arrange
        var name = _fixture.Create<string>();

        // Act
        var sut = new WkHtmlAttribute(name);

        // Assert
        sut.Name.Should().Be(name);
    }
}
