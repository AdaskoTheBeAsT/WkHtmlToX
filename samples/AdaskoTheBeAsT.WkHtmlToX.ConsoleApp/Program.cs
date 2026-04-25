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
#pragma warning disable CC0061 // Asynchronous method can be terminated with the 'Async' keyword.
        private static async Task Main()
#pragma warning restore CC0061 // Asynchronous method can be terminated with the 'Async' keyword.
        {
            var htmlToPdfGenerator = new HtmlToPdfDocumentGenerator(new SmallHtmlGenerator());
            var configuration = new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, runtimeIdentifier: null);
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
#pragma warning disable MA0042 // File creation should be wrapped in a 'using' statement
#pragma warning disable RCS1261 // Dispose object before losing scope
                using var stream = new FileStream(
                    Path.Combine("Files", $"{DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture)}.pdf"),
                    FileMode.Create);
#pragma warning restore RCS1261 // Dispose object before losing scope
#pragma warning restore MA0042 // File creation should be wrapped in a 'using' statement
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
