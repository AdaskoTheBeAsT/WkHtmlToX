using System;
using System.IO;

namespace AdaskoTheBeAsT.WkHtmlToX.Abstractions
{
    public interface IHtmlToImageConverter
        : IConverter
    {
        /// <summary>
        ///  Converts document based on given settings.
        /// </summary>
        /// <param name="document">Document to convert.</param>
        /// <param name="createStreamFunc">Creation <see cref="Stream"/> function based on length.</param>
        /// <returns>True if converted.</returns>
        bool Convert(IHtmlToImageDocument document, Func<int, Stream> createStreamFunc);
    }
}
