using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.WorkItems;

namespace AdaskoTheBeAsT.WkHtmlToX;

public class ImageConverter(IWkHtmlToXEngine engine)
    : IImageConverter
{
    private readonly IWkHtmlToXEngine _engine = engine ?? throw new ArgumentNullException(nameof(engine));

    public Task<bool> ConvertAsync(
        IHtmlToImageDocument document,
        Func<int, Stream> createStreamFunc,
        CancellationToken token)
    {
        var item = new ImageConvertWorkItem(document, createStreamFunc);
        _engine.AddConvertWorkItem(item, token);
#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
        return item.TaskCompletionSource.Task;
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks
    }
}
