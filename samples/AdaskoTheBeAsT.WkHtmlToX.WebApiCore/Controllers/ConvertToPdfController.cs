using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
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
        private readonly IHtmlToPdfDocumentGenerator _htmlToPdfDocumentGenerator;
        private readonly IHtmlToPdfAsyncConverter _htmlToPdfAsyncConverter;

        public ConvertToPdfController(
            IHtmlToPdfDocumentGenerator htmlToPdfDocumentGenerator,
            IHtmlToPdfAsyncConverter htmlToPdfAsyncConverter)
        {
            _htmlToPdfDocumentGenerator = htmlToPdfDocumentGenerator;
            _htmlToPdfAsyncConverter = htmlToPdfAsyncConverter;
        }

        [HttpPost]
        public async Task<IActionResult> Convert()
        {
            var doc = _htmlToPdfDocumentGenerator.Generate();
            var stream = await _htmlToPdfAsyncConverter.ConvertAsync(doc);
            var result = new FileStreamResult(stream, "application/pdf")
            {
                FileDownloadName = "sample.pdf",
            };

            return result;
        }
    }
}
