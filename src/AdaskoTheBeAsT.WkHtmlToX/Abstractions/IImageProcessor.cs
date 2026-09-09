using System;
using System.IO;
using System.Threading;
using AdaskoTheBeAsT.WkHtmlToX.Engine;

namespace AdaskoTheBeAsT.WkHtmlToX.Abstractions;

internal interface IImageProcessor
{
    IWkHtmlToImageModule ImageModule { get; }

    ISettings? ProcessingDocument { get; }

    bool Convert(IHtmlToImageDocument? document, Func<int, Stream> createStreamFunc);

    ConversionResult ConvertWithResult(IHtmlToImageDocument? document, Func<int, Stream> createStreamFunc, CancellationToken cancellationToken);
}
