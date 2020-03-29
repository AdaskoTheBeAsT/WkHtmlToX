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
            Stream stream)
        {
            Document = document ?? throw new ArgumentNullException(nameof(document));
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));
            TaskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public IHtmlToPdfDocument Document { get; }

        public Stream Stream { get; }

        public TaskCompletionSource<bool> TaskCompletionSource { get; }
    }
}
