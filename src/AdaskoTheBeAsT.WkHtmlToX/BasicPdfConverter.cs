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

        public Stream Convert(IHtmlToPdfDocument document)
        {
            var result = Stream.Null;
            var thread = new Thread(
                () => result = ConvertImpl(document))
            {
                IsBackground = true,
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            return result;
        }

#pragma warning disable CA1031 // Do not catch general exception types
        public Task<Stream> ConvertAsync(
            IHtmlToPdfDocument document)
        {
            var tcs = new TaskCompletionSource<Stream>(TaskCreationOptions.RunContinuationsAsynchronously);
            var thread = new Thread(() =>
            {
                try
                {
                    var pdf = ConvertImpl(document);
                    tcs.SetResult(pdf);
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
