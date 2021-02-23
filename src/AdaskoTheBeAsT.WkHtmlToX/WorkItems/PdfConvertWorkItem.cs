using System;
using System.IO;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;

namespace AdaskoTheBeAsT.WkHtmlToX.WorkItems
{
    public sealed class PdfConvertWorkItem
        : ConvertWorkItemBase
    {
        public PdfConvertWorkItem(
            IHtmlToPdfDocument document,
            Func<int, Stream> streamFunc)
            : base(streamFunc)
        {
            Document = document ?? throw new ArgumentNullException(nameof(document));
        }

        public IHtmlToPdfDocument Document { get; }

        public override void Accept(IWorkItemVisitor visitor) => visitor.Visit(this);
    }
}
