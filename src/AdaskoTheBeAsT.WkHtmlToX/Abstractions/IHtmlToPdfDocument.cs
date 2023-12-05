using System.Collections.Generic;
using AdaskoTheBeAsT.WkHtmlToX.Settings;

namespace AdaskoTheBeAsT.WkHtmlToX.Abstractions;

public interface IHtmlToPdfDocument
    : ISettings
{
    List<PdfObjectSettings> ObjectSettings { get; }

    PdfGlobalSettings GlobalSettings { get; set; }
}
