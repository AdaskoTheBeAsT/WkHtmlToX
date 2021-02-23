#nullable enable
using System.IO;
using System.Text;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Utils;

namespace AdaskoTheBeAsT.WkHtmlToX.Settings
{
    public class PdfObjectSettings : ISettings
    {
        /// <summary>
        /// Should we use a dotted line when creating a table of content.
        /// </summary>
        [WkHtml("toc.useDottedLines")]
        public bool? UseDottedLines { get; set; }

        /// <summary>
        /// The caption to use when creating a table of content.
        /// </summary>
        [WkHtml("toc.captionText")]
        public string? CaptionText { get; set; }

        /// <summary>
        /// Should we create links from the table of content into the actual content.
        /// </summary>
        [WkHtml("toc.forwardLinks")]
        public bool? ForwardLinks { get; set; }

        /// <summary>
        /// Should we link back from the content to this table of content.
        /// </summary>
        [WkHtml("toc.backLinks")]
        public bool? BackLinks { get; set; }

        /// <summary>
        /// The indentation used for every table of content level, e.g. "2em".
        /// </summary>
        [WkHtml("toc.indentation")]
        public string? Indentation { get; set; }

        /// <summary>
        /// How much should we scale down the font for every toc level? E.g. "0.8".
        /// </summary>
        [WkHtml("toc.fontScale")]
        public string? FontScale { get; set; }

        /// <summary>
        /// The URL or path of the web page to convert, if "-" input is read from stdin. Default = "".
        /// </summary>
        [WkHtml("page")]
        public string? Page { get; set; }

        /// <summary>
        /// Should external links in the HTML document be converted into external pdf links. Default = true.
        /// </summary>
        [WkHtml("useExternalLinks")]
        public bool? UseExternalLinks { get; set; }

        /// <summary>
        /// Should internal links in the HTML document be converted into pdf references. Default = true.
        /// </summary>
        [WkHtml("useLocalLinks")]
        public bool? UseLocalLinks { get; set; }

        /// <summary>
        /// Should we turn HTML forms into PDF forms. Default = false.
        /// </summary>
        [WkHtml("produceForms")]
        public bool? ProduceForms { get; set; }

        /// <summary>
        /// Should the sections from this document be included in the outline and table of content. Default = false.
        /// </summary>
        [WkHtml("includeInOutline")]
        public bool? IncludeInOutline { get; set; }

        /// <summary>
        /// Should we count the pages of this document, in the counter used for TOC, headers and footers. Default = false.
        /// </summary>
        [WkHtml("pagesCount")]
        public bool? PagesCount { get; set; }

        /// <summary>
        /// If not empty this object is a table of content object,
        /// "page" is ignored and this xsl style sheet is used to convert
        /// the outline XML into a table of content.
        /// </summary>
        [WkHtml("tocXsl")]
        public string? Xsl { get; set; }

        public string? HtmlContent { get; set; }

#pragma warning disable CA1819 // Properties should not return arrays
        public byte[]? HtmlContentByteArray { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

        public Stream? HtmlContentStream { get; set; }

        public Encoding? Encoding { get; set; }

        public WebSettings WebSettings { get; set; } = new();

        [WkHtml("header")]
        public SectionSettings HeaderSettings { get; set; } = new();

        [WkHtml("footer")]
        public SectionSettings FooterSettings { get; set; } = new();

        public LoadSettings LoadSettings { get; set; } = new();
    }
}
