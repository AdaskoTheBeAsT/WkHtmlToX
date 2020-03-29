using System.IO;

namespace AdaskoTheBeAsT.WkHtmlToX.Abstractions
{
    public interface IHtmlToPdfConverter
    {
        /// <summary>
        ///  Converts document based on given settings.
        /// </summary>
        /// <param name="document">Document to convert.</param>
        /// <param name="stream">Converted document in <see cref="Stream"/>.</param>
        /// <returns>True if document converted.</returns>
        bool Convert(IHtmlToPdfDocument document, Stream stream);
    }
}
