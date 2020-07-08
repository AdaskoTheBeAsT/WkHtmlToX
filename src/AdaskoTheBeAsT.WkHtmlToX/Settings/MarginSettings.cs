#nullable enable
using System.Globalization;

namespace AdaskoTheBeAsT.WkHtmlToX.Settings
{
    public class MarginSettings
    {
        public MarginSettings()
        {
            Unit = Unit.Millimeters;
        }

        public MarginSettings(double top, double right, double bottom, double left)
            : this()
        {
            Top = top;

            Bottom = bottom;

            Left = left;

            Right = right;
        }

        public Unit Unit { get; set; }

        public double? Top { get; set; }

        public double? Bottom { get; set; }

        public double? Left { get; set; }

        public double? Right { get; set; }

        public string? GetMarginValue(double? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            var strUnit = Unit switch
            {
                Unit.Inches => "in",
                Unit.Millimeters => "mm",
                Unit.Centimeters => "cm",
                _ => "in",
            };

            return $"{value.Value.ToString("0.##", CultureInfo.InvariantCulture)}{strUnit}";
        }
    }
}
