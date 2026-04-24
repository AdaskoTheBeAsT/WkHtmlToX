using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.WorkItems;

namespace AdaskoTheBeAsT.WkHtmlToX;

public class ImageConverter
    : IImageConverter
{
    private readonly IWkHtmlToXEngine _engine;

    public ImageConverter(IWkHtmlToXEngine engine)
    {
#if !NET8_0_OR_GREATER
        if (engine == null)
        {
            throw new ArgumentNullException(nameof(engine));
        }
#endif
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(engine);
#endif
        _engine = engine;
    }

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
