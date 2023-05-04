using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.BusinessLogic;
using AdaskoTheBeAsT.WkHtmlToX.Engine;

namespace AdaskoTheBeAsT.WkHtmlToX.ConsoleApp
{
    internal static class Program
    {
        private static async Task Main()
        {
            var htmlToPdfGenerator = new HtmlToPdfDocumentGenerator(new SmallHtmlGenerator());
            var configuration = new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, null);
            using (var engine = new WkHtmlToXEngine(configuration))
            {
                engine.Initialize();
                var doc = htmlToPdfGenerator.Generate();

                if (!Directory.Exists("files"))
                {
                    Directory.CreateDirectory("files");
                }

                var converter = new PdfConverter(engine);
#pragma warning disable SEC0112 // Path Tampering Unvalidated File Path
#pragma warning disable SCS0018 // Potential Path Traversal vulnerability was found where '{0}' in '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.
#pragma warning disable CC0022 // Should dispose object
                using var stream = new FileStream(
                    Path.Combine("Files", $"{DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture)}.pdf"),
                    FileMode.Create);
#pragma warning restore CC0022 // Should dispose object
#pragma warning restore SCS0018 // Potential Path Traversal vulnerability was found where '{0}' in '{1}' may be tainted by user-controlled data from '{2}' in method '{3}'.
#pragma warning restore SEC0112 // Path Tampering Unvalidated File Path
#pragma warning disable IDISP011
                var converted = await converter.ConvertAsync(doc, _ => stream, CancellationToken.None).ConfigureAwait(false);
#pragma warning restore IDISP011
                Console.WriteLine(converted);
            }

            Console.ReadKey();
        }
    }
}
