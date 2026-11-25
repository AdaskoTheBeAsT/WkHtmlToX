using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using Microsoft.IO;

namespace ImageConsoleApp
{
    internal static class Program
    {
#pragma warning disable CC0061 // Asynchronous method can be terminated with the 'Async' keyword.
        private static async Task<int> Main(string[] args)
#pragma warning restore CC0061 // Asynchronous method can be terminated with the 'Async' keyword.
        {
            if (args.Length != 2)
            {
                await Console.Error.WriteLineAsync("Usage: ImageConsoleApp <input.html> <output.jpg>").ConfigureAwait(false);
                return 1;
            }

            var input = new Uri(Path.GetFullPath(args[0])).AbsoluteUri;
#pragma warning disable CC0022 // The configured async-using scope owns the engine.
            var engine = new WkHtmlToXEngine(new WkHtmlToXConfiguration());
#pragma warning restore CC0022
            await using var engineScope = engine.ConfigureAwait(false);
            await engine.InitializeAsync(CancellationToken.None).ConfigureAwait(false);

            var doc = new HtmlToImageDocument
            {
                ImageSettings =
                {
                    In = input,
                    Format = "jpg",
                },
            };

            var streamManager = new RecyclableMemoryStreamManager();
            var stream = streamManager.GetStream();
            await using var streamScope = stream.ConfigureAwait(false);
#pragma warning disable IDISP011 // The converter borrows the output stream.
            var result = await engine.ConvertImageAsync(doc, _ => stream, CancellationToken.None)
                .ConfigureAwait(false);
#pragma warning restore IDISP011
            if (!result.Success)
            {
                await Console.Error.WriteLineAsync($"Conversion failed: {result.FailureKind}").ConfigureAwait(false);
                return 1;
            }

#pragma warning disable SCS0018, SEC0112 // A local CLI user explicitly selects the output path.
            var output = new FileStream(args[1], FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, useAsync: true);
#pragma warning restore SCS0018, SEC0112
            await using var outputScope = output.ConfigureAwait(false);
            stream.Position = 0;
            await stream.CopyToAsync(output, CancellationToken.None).ConfigureAwait(false);
            return 0;
        }
    }
}
