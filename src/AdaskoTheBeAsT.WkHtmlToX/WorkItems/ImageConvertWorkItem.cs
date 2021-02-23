using System;
using System.IO;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;

namespace AdaskoTheBeAsT.WkHtmlToX.WorkItems
{
    public sealed class ImageConvertWorkItem
        : ConvertWorkItemBase
    {
        public ImageConvertWorkItem(
            IHtmlToImageDocument document,
            Func<int, Stream> streamFunc)
            : base(streamFunc)
        {
            Document = document ?? throw new ArgumentNullException(nameof(document));
        }

        public IHtmlToImageDocument Document { get; }

        public override void Accept(IWorkItemVisitor visitor) => visitor.Visit(this);
    }
}
