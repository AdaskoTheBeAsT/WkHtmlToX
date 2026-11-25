using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.Hosting;
using AdaskoTheBeAsT.WkHtmlToX.Settings;
using AdaskoTheBeAsT.WkHtmlToX.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AdaskoTheBeAsT.WkHtmlToX.NanoApi;

internal static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddWkHtmlToXHostedService(new WkHtmlToXConfiguration
        {
            NativeLibraryPath = builder.Configuration.GetValue<string>("NativeLibraryPath"),
            WorkerOptions = new WkHtmlToXWorkerOptions
            {
                MaxOperationsPerSession = builder.Configuration.GetValue<int>("MaxOperationsPerSession"),
            },
        });
        builder.Services.AddHealthChecks();

        var app = builder.Build();
        app.MapHealthChecks("/health");
        app.MapGet("/pdf", RenderPdfAsync);
        app.Run();
    }

    private static async Task<Results<FileContentHttpResult, ProblemHttpResult>> RenderPdfAsync(
        IWkHtmlToXAsyncEngine engine,
        CancellationToken cancellationToken)
    {
        var platform = OperatingSystem.IsWindows() ? "Windows" : "Linux";
        var document = new HtmlToPdfDocument
        {
            GlobalSettings = new PdfGlobalSettings { PaperSize = PaperKind.A4 },
            ObjectSettings =
            {
                new PdfObjectSettings { HtmlContent = $"<html><body><h1>WkHtmlToX on {platform}</h1></body></html>" },
            },
        };
#pragma warning disable RCS1261, MA0042 // In-memory stream disposal does not block on I/O.
        using var output = new MemoryStream();
#pragma warning restore RCS1261, MA0042
#pragma warning disable IDISP011 // The converter writes to but does not own the stream.
        var result = await engine.ConvertPdfAsync(document, _ => output, cancellationToken).ConfigureAwait(false);
#pragma warning restore IDISP011
        if (!result.Success || output.Length == 0)
        {
            return TypedResults.Problem("The native renderer did not produce a PDF.");
        }

        var content = output.ToArray();
        return TypedResults.File(content, "application/pdf", "wkhtmltox.pdf");
    }
}
