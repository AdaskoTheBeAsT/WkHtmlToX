using System.Collections.Generic;
using AdaskoTheBeAsT.WkHtmlToX.Settings;

namespace AdaskoTheBeAsT.WkHtmlToX.Abstractions
{
    public interface IHtmlToPdfDocument
        : IDocument
    {
        List<PdfObjectSettings> ObjectSettings { get; }

        PdfGlobalSettings GlobalSettings { get; set; }
    }
}
