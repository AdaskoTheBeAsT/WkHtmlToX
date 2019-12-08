using AdaskoTheBeAsT.WkHtmlToX.Settings;

namespace AdaskoTheBeAsT.WkHtmlToX.Abstractions
{
    public interface IHtmlToImageDocument
        : IDocument
    {
        ImageSettings ImageSettings { get; set; }
    }
}
