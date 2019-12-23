using AdaskoTheBeAsT.WkHtmlToX.BusinessLogic;
using Microsoft.AspNetCore.Mvc;

namespace AdaskoTheBeAsT.WkHtmlToX.WebApiCore.Controllers
{
    [Produces("application/pdf")]
    [ApiController]
    [Route("api/[controller]")]
    public class ConvertToPdfController
        : ControllerBase
    {
        [HttpPost]
        public IActionResult Convert()
        {
            var htmlToPdfGenerator = new HtmlToPdfDocumentGenerator(new SmallHtmlGenerator());
            var doc = htmlToPdfGenerator.Generate();
            using var converter = new BasicPdfConverter();
            var stream = converter.Convert(doc);
            var result = new FileStreamResult(stream, "application/pdf")
            {
                FileDownloadName = "sample.pdf",
            };

            return result;
        }
    }
}
