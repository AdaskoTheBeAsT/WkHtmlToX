using System.IO;
using System.Threading.Tasks;

namespace AdaskoTheBeAsT.WkHtmlToX.Abstractions
{
    public interface IHtmlToPdfAsyncConverter
    {
        /// <summary>
        ///  Converts document based on given settings.
        /// </summary>
        /// <param name="document">Document to convert.</param>
        /// <param name="stream">Converted document in <see cref="Stream"/>.</param>
        Task<bool> ConvertAsync(IHtmlToPdfDocument document, Stream stream);
    }
}
