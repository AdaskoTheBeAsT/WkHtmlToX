using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Settings;

namespace AdaskoTheBeAsT.WkHtmlToX.Documents;

public class HtmlToImageDocument
    : IHtmlToImageDocument
{
    public HtmlToImageDocument()
    {
        ImageSettings = new ImageSettings();
    }

    public ImageSettings ImageSettings { get; set; }
}
