#nullable enable
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Utils;

namespace AdaskoTheBeAsT.WkHtmlToX.Settings
{
    public class SectionSettings : ISettings
    {
        /// <summary>
        /// The font size to use for the section. Default = 12.
        /// </summary>
        [WkHtml("fontSize")]
        public int? FontSize { get; set; }

        /// <summary>
        /// The name of the font to use for the section. Default = "Ariel".
        /// </summary>
        [WkHtml("fontName")]
        public string? FontName { get; set; }

        /// <summary>
        /// The string to print in the left part of the section,
        /// note that some sequences are replaced in this string,
        /// see the wkhtmltopdf manual. Default = "".
        /// </summary>
        [WkHtml("left")]
        public string? Left { get; set; }

        /// <summary>
        /// The text to print in the right part of the section,
        /// note that some sequences are replaced in this string,
        /// see the wkhtmltopdf manual. Default = "".
        /// </summary>
        [WkHtml("center")]
        public string? Center { get; set; }

        /// <summary>
        /// The text to print in the right part of the section,
        /// note that some sequences are replaced in this string,
        /// see the wkhtmltopdf manual. Default = "".
        /// </summary>
        [WkHtml("right")]
        public string? Right { get; set; }

        /// <summary>
        /// Whether a line should be printed in section (under the header or above the footer). Default = false.
        /// </summary>
        [WkHtml("line")]
        public bool? Line { get; set; }

        /// <summary>
        /// The amount of space to put between the section and the content, e.g. "1.8".
        /// Be aware that if this is too large the section will be printed outside the pdf document.
        /// This can be corrected
        /// for header with the margin.top setting.
        /// for footer with the margin.bottom setting.
        /// Default = 0.00.
        /// </summary>
        [WkHtml("spacing")]
        public double? Spacing { get; set; }

        /// <summary>
        /// Url for a HTML document to use for the section. Default = "".
        /// </summary>
        [WkHtml("htmlUrl")]
#pragma warning disable CA1056 // Uri properties should not be strings
        public string? HtmlUrl { get; set; }
#pragma warning restore CA1056 // Uri properties should not be strings
    }
}
