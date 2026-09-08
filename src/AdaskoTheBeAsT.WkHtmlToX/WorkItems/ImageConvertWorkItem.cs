using System;
using System.IO;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;

namespace AdaskoTheBeAsT.WkHtmlToX.WorkItems;

#pragma warning disable S1133 // Planned removal in the next major release; retained for migration.
[Obsolete("Use IWkHtmlToXAsyncEngine.ConvertImageAsync instead.")]
#pragma warning restore S1133
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
