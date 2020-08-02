using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AdaskoTheBeAsT.WkHtmlToX.Settings;
using AdaskoTheBeAsT.WkHtmlToX.Utils;

namespace AdaskoTheBeAsT.WkHtmlToX.MemoryTest
{
    public class HtmlToPdfDocumentGenerator
        : IHtmlToPdfDocumentGenerator
    {
        private readonly IHtmlGenerator _htmlGenerator;

        public HtmlToPdfDocumentGenerator(
            IHtmlGenerator htmlGenerator)
        {
            _htmlGenerator = htmlGenerator;
        }

        public HtmlToPdfDocument Generate()
        {
            var doc = new HtmlToPdfDocument
            {
                GlobalSettings =
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Landscape,
                    PaperSize = PaperKind.A4,
                },
                ObjectSettings =
                {
                    new PdfObjectSettings
                    {
                        PagesCount = true,
                        WebSettings =
                        {
                            DefaultEncoding = "utf-8",
                        },
                        HeaderSettings =
                        {
                            FontSize = 9, Right = "Page [page] of [toPage]", Line = true,
                        },
                        FooterSettings =
                        {
                            FontSize = 9, Right = "Page [page] of [toPage]",
                        },
                    },
                },
            };

            doc.ObjectSettings[0].HtmlContent = _htmlGenerator.Generate();

            return doc;
        }
    }
}
