using AdaskoTheBeAsT.WkHtmlToX.Settings;

namespace AdaskoTheBeAsT.WkHtmlToX.Abstractions
{
    public interface IHtmlToImageDocument
        : ISettings
    {
        ImageSettings ImageSettings { get; set; }
    }
}
