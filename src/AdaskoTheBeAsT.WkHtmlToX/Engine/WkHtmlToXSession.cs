using System;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;

namespace AdaskoTheBeAsT.WkHtmlToX.Engine;

internal sealed class WkHtmlToXSession
{
    public WkHtmlToXSession(
        ILibraryLoader loader,
        IPdfProcessor pdfProcessor,
        IImageProcessor imageProcessor)
    {
        Loader = loader ?? throw new ArgumentNullException(nameof(loader));
        PdfProcessor = pdfProcessor ?? throw new ArgumentNullException(nameof(pdfProcessor));
        ImageProcessor = imageProcessor ?? throw new ArgumentNullException(nameof(imageProcessor));
    }

    public ILibraryLoader Loader { get; }

    public IPdfProcessor PdfProcessor { get; }

    public IImageProcessor ImageProcessor { get; }
}
