using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;

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

        internal BasicPdfConverter(
            IWkHtmlToXModuleFactory moduleFactory,
            IWkHtmlToPdfModuleFactory pdfModuleFactory)
            : base(moduleFactory, pdfModuleFactory)
        {
        }

        public bool Convert(IHtmlToPdfDocument document, Stream stream)
        {
            if (document is null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (stream is null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var converted = false;

            var thread = new Thread(
                () => converted = ConvertImpl(document, stream))
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
            Stream stream)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var thread = new Thread(() =>
            {
                try
                {
                    var converted = ConvertImpl(document, stream);
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
