using System;
using System.IO;

namespace AdaskoTheBeAsT.WkHtmlToX.Abstractions;

internal interface IImageProcessor
{
    IWkHtmlToImageModule ImageModule { get; }

    ISettings? ProcessingDocument { get; }

    bool Convert(IHtmlToImageDocument? document, Func<int, Stream> createStreamFunc);
}
