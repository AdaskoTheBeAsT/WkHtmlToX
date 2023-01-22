using AdaskoTheBeAsT.WkHtmlToX;
using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using Microsoft.IO;

namespace ImageConsoleApp
{
    internal static class Program
    {
        private static async Task Main()
        {
            var streamManager = new RecyclableMemoryStreamManager();
            var configuration = new WkHtmlToXConfiguration(
                (int)Environment.OSVersion.Platform,
                runtimeIdentifier: null);

            using var engine = new WkHtmlToXEngine(configuration);
            engine.Initialize();

            var doc = new HtmlToImageDocument
            {
                ImageSettings =
                {
                    In = "https://adaskothebeast.com/",
                    Format = "jpg",
                    Out = "c:\\temp\\google.jpg",
                    WebSettings =
                    {
                        LoadImages = true,
                        EnableJavascript = true,
                        PrintMediaType = true,
                        EnablePlugins = true,
                    },
                    LoadSettings =
                    {
                        JSDelay = 1200,
                    },
                },
            };

            var converter = new ImageConverter(engine);
            Stream? stream = null;

            Console.WriteLine("before convert");

            var converted = await converter.ConvertAsync(
                doc,
                length =>
                {
                    stream?.Dispose();
                    stream = streamManager.GetStream(
                        Guid.NewGuid(),
                        "wkhtmltox",
                        length);

                    return stream;
                },
                CancellationToken.None)
                .ConfigureAwait(false);

            Console.WriteLine("converted: " + converted);
        }
    }
}
