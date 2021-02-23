using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.WorkItems;

namespace AdaskoTheBeAsT.WkHtmlToX
{
    public class PdfConverter
        : IPdfConverter
    {
        private readonly IWkHtmlToXEngine _engine;

        public PdfConverter(IWkHtmlToXEngine engine)
        {
            _engine = engine;
        }

        public Task<bool> ConvertAsync(
            IHtmlToPdfDocument document,
            Func<int, Stream> createStreamFunc,
            CancellationToken token)
        {
            var item = new PdfConvertWorkItem(document, createStreamFunc);
            _engine.AddConvertWorkItem(item, token);
            return item.TaskCompletionSource.Task;
        }
    }
}
