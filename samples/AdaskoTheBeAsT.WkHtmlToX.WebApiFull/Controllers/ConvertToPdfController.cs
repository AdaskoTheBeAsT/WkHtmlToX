using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.BusinessLogic;

namespace AdaskoTheBeAsT.WkHtmlToX.WebApiFull.Controllers
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
        public HttpResponseMessage Post()
        {
            var doc = _htmlToPdfDocumentGenerator.Generate();
            var stream = _htmlToPdfAsyncConverter.ConvertAsync(doc).GetAwaiter().GetResult();
            var httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
            httpResponseMessage.Content = new StreamContent(stream);
            httpResponseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "sample.pdf",
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return httpResponseMessage;
        }
    }
}
