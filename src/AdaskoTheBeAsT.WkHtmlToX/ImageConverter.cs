#pragma warning disable CS0618 // Intentional legacy compatibility implementation or regression coverage.
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
        if (_engine is IWkHtmlToXAsyncEngine asyncEngine)
        {
            return WkHtmlToXEngine.ToLegacyResultAsync(asyncEngine.ConvertImageAsync(document, createStreamFunc, token));
        }

        var item = new ImageConvertWorkItem(document, createStreamFunc);
        _engine.AddConvertWorkItem(item, token);
#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
        return item.TaskCompletionSource.Task;
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks
    }
}

#pragma warning restore CS0618
