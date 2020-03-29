using System;
using System.IO;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;

namespace AdaskoTheBeAsT.WkHtmlToX.WorkItems
{
    internal class PdfConvertWorkItem
    {
        public PdfConvertWorkItem(
            IHtmlToPdfDocument document,
            Func<int, Stream> streamFunc)
        {
            Document = document ?? throw new ArgumentNullException(nameof(document));
            StreamFunc = streamFunc ?? throw new ArgumentNullException(nameof(streamFunc));
            TaskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public IHtmlToPdfDocument Document { get; }

        public Func<int, Stream> StreamFunc { get; }

        public TaskCompletionSource<bool> TaskCompletionSource { get; }
    }
}
