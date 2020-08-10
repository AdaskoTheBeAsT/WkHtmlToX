using System;
using System.Globalization;
using System.IO;
using AdaskoTheBeAsT.WkHtmlToX.BusinessLogic;
using AdaskoTheBeAsT.WkHtmlToX.Loaders;

namespace AdaskoTheBeAsT.WkHtmlToX.ConsoleApp
{
    internal static class Program
    {
        private static void Main()
        {
            var libFactory = new LibraryLoaderFactory();
            var htmlToPdfGenerator = new HtmlToPdfDocumentGenerator(new SmallHtmlGenerator());
            using (var libraryLoader = libFactory.Create((int)Environment.OSVersion.Platform, null))
            {
                libraryLoader.Load();
                var doc = htmlToPdfGenerator.Generate();

                if (!Directory.Exists("files"))
                {
                    Directory.CreateDirectory("files");
                }

#pragma warning disable CC0022 // Should dispose object
                using var converter = new BasicPdfConverter();
#pragma warning disable SEC0112 // Path Tampering Unvalidated File Path
                using var stream = new FileStream(
                    Path.Combine("Files", $"{DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture)}.pdf"),
                    FileMode.Create);
#pragma warning restore SEC0112 // Path Tampering Unvalidated File Path
#pragma warning restore CC0022 // Should dispose object
                var converted = converter.Convert(doc, _ => stream);
                Console.WriteLine(converted);
            }

            Console.ReadKey();
        }
    }
}
