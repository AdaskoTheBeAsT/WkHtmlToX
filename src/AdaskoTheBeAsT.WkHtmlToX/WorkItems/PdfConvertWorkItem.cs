using System.IO;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;

namespace AdaskoTheBeAsT.WkHtmlToX.WorkItems
{
    internal class PdfConvertWorkItem
    {
        public PdfConvertWorkItem(
            IHtmlToPdfDocument document)
        {
            Document = document;
            TaskCompletionSource = new TaskCompletionSource<Stream>();
        }

        public IHtmlToPdfDocument Document { get; }

        public TaskCompletionSource<Stream> TaskCompletionSource { get; }
    }
}
