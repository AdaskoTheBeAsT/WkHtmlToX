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
        /// <param name="stream">Converted document in <see cref="Stream"/>.</param>
        /// <returns>True if converted.</returns>
        bool Convert(IHtmlToImageDocument document, Stream stream);
    }
}
