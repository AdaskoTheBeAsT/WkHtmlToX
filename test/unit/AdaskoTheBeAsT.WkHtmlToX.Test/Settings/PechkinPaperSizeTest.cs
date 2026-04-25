using System;
using System.Collections.Generic;
using System.Linq;
using AdaskoTheBeAsT.WkHtmlToX.Settings;
using AdaskoTheBeAsT.WkHtmlToX.Utils;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Settings;

public sealed class PechkinPaperSizeTest
{
    public static IEnumerable<PaperKind> GetConvertiblePaperKinds()
    {
#if NET5_0_OR_GREATER
        var paperKinds = Enum.GetValues<PaperKind>();
#else
        var paperKinds = Enum.GetValues(typeof(PaperKind)).Cast<PaperKind>();
#endif
        return paperKinds.Where(pk => pk != PaperKind.Custom);
    }

    [Fact]
    public void FromPaperKindShouldThrowExceptionWhenPaperKindUnknown()
    {
        // Arrange
        // ReSharper disable once AssignmentIsFullyDiscarded
        Action action = () => _ = PechkinPaperSize.FromPaperKind(PaperKind.Custom);

        // Act and Assert
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void FromPaperKindShouldReturnNonEmptyValueWhenPaperKindConvertible()
    {
        // Arrange
        var paperKinds = GetConvertiblePaperKinds();

        // Act and Assert
        using (new AssertionScope())
        {
            foreach (var paperKind in paperKinds)
            {
                var pechkinPaperSize = PechkinPaperSize.FromPaperKind(paperKind);
                pechkinPaperSize.Should().NotBeNull();
                pechkinPaperSize.Width.Should().NotBeNullOrWhiteSpace();
                pechkinPaperSize.Height.Should().NotBeNullOrWhiteSpace();
            }
        }
    }

    [Fact]
    public void ImplicitCastShouldThrowExceptionWhenPaperKindUnknown()
    {
        // Arrange
        Action action = () =>
        {
#pragma warning disable S1481 // Unused local variables should be removed
#pragma warning disable 219
            PechkinPaperSize pps = PaperKind.Custom;
#pragma warning restore 219
#pragma warning restore S1481 // Unused local variables should be removed
        };

        // Act and Assert
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ImplicitCastShouldReturnNonEmptyValueWhenPaperKindConvertible()
    {
        // Arrange
        var paperKinds = GetConvertiblePaperKinds();

        // Act and Assert
        using (new AssertionScope())
        {
            foreach (var paperKind in paperKinds)
            {
                PechkinPaperSize pechkinPaperSize = paperKind;
                pechkinPaperSize.Should().NotBeNull();
                pechkinPaperSize.Width.Should().NotBeNullOrWhiteSpace();
                pechkinPaperSize.Height.Should().NotBeNullOrWhiteSpace();
            }
        }
    }
}
