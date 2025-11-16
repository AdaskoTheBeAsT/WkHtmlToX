using System.Collections.Generic;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Settings;

namespace AdaskoTheBeAsT.WkHtmlToX.Documents;

public class HtmlToPdfDocument
    : IHtmlToPdfDocument
{
    public HtmlToPdfDocument()
    {
        GlobalSettings = new PdfGlobalSettings();
        ObjectSettings = [];
    }

    public List<PdfObjectSettings> ObjectSettings { get; }

    public PdfGlobalSettings GlobalSettings { get; set; }
}
