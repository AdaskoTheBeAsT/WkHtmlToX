using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AdaskoTheBeAsT.WkHtmlToX.Abstractions
{
    public interface IImageConverter
    {
        Task<bool> ConvertAsync(
            IHtmlToImageDocument document,
            Func<int, Stream> createStreamFunc,
            CancellationToken token);
    }
}
