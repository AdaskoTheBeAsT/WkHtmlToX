using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using JetBrains.Annotations;

namespace AdaskoTheBeAsT.WkHtmlToX
{
    public class BasicPdfConverter
        : PdfConverterBase,
            IHtmlToPdfConverter,
            IHtmlToPdfAsyncConverter
    {
        public BasicPdfConverter()
        {
        }

        internal BasicPdfConverter(IWkHtmlToXModuleFactory moduleFactory, IWkHtmlToPdfModule pdfModule)
            : base(moduleFactory, pdfModule)
        {
        }

        public bool Convert(
            IHtmlToPdfDocument document,
            [InstantHandle] Func<int, Stream> createStreamFunc)
        {
            if (document is null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (createStreamFunc is null)
            {
                throw new ArgumentNullException(nameof(createStreamFunc));
            }

            var converted = false;

            var thread = new Thread(
                () => converted = ConvertImpl(
                    document,
                    createStreamFunc))
            {
                IsBackground = true,
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            return converted;
        }

#pragma warning disable CA1031 // Do not catch general exception types
        public Task<bool> ConvertAsync(
            IHtmlToPdfDocument document,
            Func<int, Stream> createStreamFunc,
            CancellationToken token)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var thread = new Thread(() =>
            {
                try
                {
                    var converted = ConvertImpl(document, createStreamFunc);
                    tcs.SetResult(converted);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            })
            {
                IsBackground = true,
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }
}
