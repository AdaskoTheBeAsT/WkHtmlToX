using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
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
#pragma warning disable CC0033 // Dispose Fields Properly
        private readonly BlockingCollection<PdfConvertWorkItem> _blockingCollection = new BlockingCollection<PdfConvertWorkItem>();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
#pragma warning restore CC0033 // Dispose Fields Properly

        public SynchronizedPdfConverter()
        {
            Initialize();
        }

        internal SynchronizedPdfConverter(IWkHtmlToXModuleFactory moduleFactory, IWkHtmlToPdfModule pdfModule)
            : base(moduleFactory, pdfModule)
        {
            Initialize();
        }

        public Task<bool> ConvertAsync(
            IHtmlToPdfDocument document,
            Func<int, Stream> createStreamFunc,
            CancellationToken token)
        {
            var item = new PdfConvertWorkItem(document, createStreamFunc);
            _blockingCollection.Add(item, token);
            return item.TaskCompletionSource.Task;
        }

        protected override void Dispose(
            bool disposing)
        {
            if (disposing)
            {
                _blockingCollection.CompleteAdding();
                _cancellationTokenSource.Cancel();
                _blockingCollection.Dispose();
                _cancellationTokenSource.Dispose();
            }

            base.Dispose(disposing);
        }

        private void Initialize()
        {
            var thread = new Thread(Process)
            {
                IsBackground = true,
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                thread.SetApartmentState(ApartmentState.STA);
            }

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
                        var converted = ConvertImpl(pdfConvertWorkItem.Document, pdfConvertWorkItem.StreamFunc);
                        pdfConvertWorkItem.TaskCompletionSource.SetResult(converted);
                    }
                    catch (Exception e)
                    {
                        pdfConvertWorkItem.TaskCompletionSource.SetException(e);
                    }
                }
            }
#pragma warning disable CC0004 // Catch block cannot be empty
            catch (OperationCanceledException)
            {
                // no op
            }
#pragma warning restore CC0004 // Catch block cannot be empty
        }
#pragma warning restore S108 // Nested blocks of code should not be left empty
#pragma warning restore CA1031 // Do not catch general exception types
    }
}
