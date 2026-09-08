using System;
using System.IO;
using System.Threading;
using AdaskoTheBeAsT.WkHtmlToX.Engine;

namespace AdaskoTheBeAsT.WkHtmlToX.Abstractions;

internal interface IPdfProcessor
{
    IWkHtmlToPdfModule PdfModule { get; }

    ISettings? ProcessingDocument { get; }

    bool Convert(IHtmlToPdfDocument document, Func<int, Stream> createStreamFunc);

    ConversionResult ConvertWithResult(IHtmlToPdfDocument document, Func<int, Stream> createStreamFunc, CancellationToken cancellationToken);
}
