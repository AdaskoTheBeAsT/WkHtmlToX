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
        /// <returns>Returns converted document in <see cref="Stream"/>.</returns>
        Task<Stream> ConvertAsync(IHtmlToPdfDocument document);
    }
}
