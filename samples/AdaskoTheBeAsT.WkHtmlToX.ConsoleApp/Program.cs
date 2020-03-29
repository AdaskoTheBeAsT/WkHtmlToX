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

                using (var converter = new BasicPdfConverter())
                using (var stream = new FileStream(
                    Path.Combine("Files", $"{DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture)}.pdf"),
                    FileMode.Create))
                {
                    var converted = converter.Convert(doc, length => stream);
                    Console.WriteLine(converted);
                }
            }

            Console.ReadKey();
        }
    }
}
