using System;
using System.IO;

namespace AdaskoTheBeAsT.WkHtmlToX.Abstractions
{
    internal interface IPdfProcessor
    {
        IWkHtmlToPdfModule WkHtmlToPdfModule { get; }

        ISettings? ProcessingDocument { get; }

        bool Convert(IHtmlToPdfDocument document, Func<int, Stream> createStreamFunc);
    }
}
