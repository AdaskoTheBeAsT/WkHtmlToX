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
        /// <returns>Returns converted document in <see cref="Stream"/>.</returns>
        Stream Convert(IHtmlToImageDocument document);
    }
}
