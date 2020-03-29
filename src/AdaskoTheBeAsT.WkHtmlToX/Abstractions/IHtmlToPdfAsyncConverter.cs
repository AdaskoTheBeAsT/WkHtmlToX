using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AdaskoTheBeAsT.WkHtmlToX.Abstractions
{
    public interface IHtmlToPdfAsyncConverter
    {
        /// <summary>
        ///  Converts document based on given settings.
        /// </summary>
        /// <param name="document">Document to convert.</param>
        /// <param name="createStreamFunc">Creation <see cref="Stream"/> function based on length.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>True if converted.</returns>
        Task<bool> ConvertAsync(
            IHtmlToPdfDocument document,
            Func<int, Stream> createStreamFunc,
            CancellationToken token);
    }
}
