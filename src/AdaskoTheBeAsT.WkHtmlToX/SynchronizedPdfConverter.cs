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
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

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
            base.Dispose(disposing);
            if (disposing)
            {
                _blockingCollection.CompleteAdding();
                _cancellationTokenSource.Cancel();
                _blockingCollection.Dispose();
                _cancellationTokenSource.Dispose();
            }
        }

        private void Initialize()
        {
            var thread = new Thread(Process)
            {
                IsBackground = true,
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start(_cancellationTokenSource.Token);
        }

#pragma warning disable CA1031 // Do not catch general exception types
        private void Process(object token)
        {
            try
            {
                foreach (var pdfConvertWorkItem in _blockingCollection.GetConsumingEnumerable((CancellationToken)token))
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
            catch (OperationCanceledException)
            {
                // noop
            }
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }
}
