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

        internal SynchronizedPdfConverter(
            IWkHtmlToXModuleFactory moduleFactory,
            IWkHtmlToPdfModuleFactory pdfModuleFactory)
            : base(moduleFactory, pdfModuleFactory)
        {
            Initialize();
        }

        public Task<bool> ConvertAsync(
            IHtmlToPdfDocument document,
            Stream stream)
        {
            var item = new PdfConvertWorkItem(document, stream);
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
#pragma warning disable S108 // Nested blocks of code should not be left empty
        private void Process(object token)
        {
            try
            {
                foreach (var pdfConvertWorkItem in _blockingCollection.GetConsumingEnumerable((CancellationToken)token))
                {
                    try
                    {
                        var converted = ConvertImpl(pdfConvertWorkItem.Document, pdfConvertWorkItem.Stream);
                        pdfConvertWorkItem.TaskCompletionSource.SetResult(converted);
                    }
                    catch (Exception e)
                    {
                        pdfConvertWorkItem.TaskCompletionSource.SetException(e);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
#pragma warning restore S108 // Nested blocks of code should not be left empty
#pragma warning restore CA1031 // Do not catch general exception types
    }
}
