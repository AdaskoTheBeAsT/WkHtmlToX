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
        public Stream Convert(IHtmlToPdfDocument document)
        {
            var pdfStream = Stream.Null;
            var thread = new Thread(
                () => pdfStream = ConvertImpl(document));
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
            thread.Join();
            return pdfStream;
        }

#pragma warning disable CA1031 // Do not catch general exception types
        public Task<Stream> ConvertAsync(
            IHtmlToPdfDocument document)
        {
            var tcs = new TaskCompletionSource<Stream>();
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
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
            return tcs.Task;
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }
}
