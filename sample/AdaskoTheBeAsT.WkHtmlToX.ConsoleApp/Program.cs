using System;
using System.Globalization;
using System.IO;
using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AdaskoTheBeAsT.WkHtmlToX.Loaders;
using AdaskoTheBeAsT.WkHtmlToX.Settings;
using AdaskoTheBeAsT.WkHtmlToX.Utils;

namespace AdaskoTheBeAsT.WkHtmlToX.ConsoleApp
{
    internal static class Program
    {
        private static void Main()
        {
            var libFactory = new LibraryLoaderFactory();
            using (var libraryLoader = libFactory.Create())
            {
                libraryLoader.Load();
                var doc = new HtmlToPdfDocument()
                {
                    GlobalSettings =
                    {
                        ColorMode = ColorMode.Color, Orientation = Orientation.Landscape, PaperSize = PaperKind.A4,
                    },
                    ObjectSettings =
                    {
                        new PdfObjectSettings()
                        {
                            PagesCount = true,
                            HtmlContent =
                                @"<p>
This paragraph
contains a lot of lines
in the source code,
but the browser 
ignores it.
</p>

<p>
This paragraph
contains      a lot of spaces
in the source     code,
but the    browser 
ignores it.
</p>

<p>
The number of lines in a paragraph depends on the size of the browser window. If you resize the browser window, the number of lines in this paragraph will change.
</p>",
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

                using (var converter = new BasicPdfConverter())
                {
                    var pdf = converter.Convert(doc);

                    if (!Directory.Exists("files"))
                    {
                        Directory.CreateDirectory("files");
                    }

                    using (var stream = new FileStream(
                        Path.Combine("Files", $"{DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture)}.pdf"),
                        FileMode.Create))
                    {
                        pdf.CopyTo(stream);
                        pdf.Dispose();
                    }
                }
            }
        }
    }
}
