using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.BusinessLogic;

namespace AdaskoTheBeAsT.WkHtmlToX.WebApiOwin.Controllers
{
    public class ConvertToPdfController : ApiController
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
        public async Task<IHttpActionResult> Post()
        {
            var doc = _htmlToPdfDocumentGenerator.Generate();
            var stream = await _htmlToPdfAsyncConverter.ConvertAsync(doc);
#pragma warning disable CA2000 // Dispose objects before losing scope
            var httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
#pragma warning restore CA2000 // Dispose objects before losing scope
            httpResponseMessage.Content = new StreamContent(stream);
            httpResponseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "sample.pdf",
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            return ResponseMessage(httpResponseMessage);
        }
    }
}
