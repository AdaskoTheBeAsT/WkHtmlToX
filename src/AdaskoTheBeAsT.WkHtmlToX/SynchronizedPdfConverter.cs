using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.WorkItems;

namespace AdaskoTheBeAsT.WkHtmlToX
{
    /// <summary>
    /// Use in multi threaded applications.
    /// Register in IoC as singleton.
    /// </summary>
    public class SynchronizedPdfConverter
        : PdfConverterBase,
            IHtmlToPdfAsyncConverter
    {
        private readonly BlockingCollection<PdfConvertWorkItem> _blockingCollection = new BlockingCollection<PdfConvertWorkItem>();

        public SynchronizedPdfConverter()
        {
            Initialize();
        }

        public Task<Stream> ConvertAsync(
            IHtmlToPdfDocument document)
        {
            var item = new PdfConvertWorkItem(document);
            _blockingCollection.Add(item);
            return item.TaskCompletionSource.Task;
        }

        protected override void Dispose(
            bool disposing)
        {
            _blockingCollection.CompleteAdding();
            base.Dispose(disposing);
            if (disposing)
            {
                _blockingCollection.Dispose();
            }
        }

        private void Initialize()
        {
            var thread = new Thread(Process);
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
        }

#pragma warning disable CA1031 // Do not catch general exception types
        private void Process()
        {
            foreach (var pdfConvertWorkItem in _blockingCollection.GetConsumingEnumerable())
            {
                try
                {
                    var pdf = ConvertImpl(pdfConvertWorkItem.Document);
                    pdfConvertWorkItem.TaskCompletionSource.SetResult(pdf);
                }
                catch (Exception e)
                {
                    pdfConvertWorkItem.TaskCompletionSource.SetException(e);
                }
            }
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }
}
