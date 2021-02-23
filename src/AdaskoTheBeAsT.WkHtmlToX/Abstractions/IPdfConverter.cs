using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AdaskoTheBeAsT.WkHtmlToX.Abstractions
{
    public interface IPdfConverter
    {
        Task<bool> ConvertAsync(
            IHtmlToPdfDocument document,
            Func<int, Stream> createStreamFunc,
            CancellationToken token);
    }
}
