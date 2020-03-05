using System;
using System.Linq;
using AdaskoTheBeAsT.WkHtmlToX.Settings;
using AdaskoTheBeAsT.WkHtmlToX.Utils;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace AdaskoTheBeAsT.WkHtmlToX.Test.Settings
{
    public sealed class PechkinPaperSizeTest
    {
        [Fact]
        public void FromPaperKindShouldThrowExceptionWhenPaperKindUnknown()
        {
            // Arrange
            Action action = () => _ = PechkinPaperSize.FromPaperKind(PaperKind.Custom);

            // Act & Assert
            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void FromPaperKindShouldReturnNonEmptyValueWhenPaperKindConvertible()
        {
            // Arrange
            var paperKinds =
                Enum.GetValues(typeof(PaperKind))
                .Cast<PaperKind>()
                .Where(pk => pk != PaperKind.Custom);

            // Act & Assert
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
                PechkinPaperSize pps = PaperKind.Custom;
#pragma warning restore S1481 // Unused local variables should be removed
            };

            // Act & Assert
            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void ImplicitCastShouldReturnNonEmptyValueWhenPaperKindConvertible()
        {
            // Arrange
            var paperKinds =
                Enum.GetValues(typeof(PaperKind))
                    .Cast<PaperKind>()
                    .Where(pk => pk != PaperKind.Custom);

            // Act & Assert
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
}
