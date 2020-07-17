#nullable enable
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.BusinessLogic;
using Microsoft.IO;

namespace AdaskoTheBeAsT.WkHtmlToX.WebApiOwin.Controllers
{
    public class ConvertToPdfController : ApiController
    {
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
        private readonly IHtmlToPdfDocumentGenerator _htmlToPdfDocumentGenerator;
        private readonly IHtmlToPdfAsyncConverter _htmlToPdfAsyncConverter;

        public ConvertToPdfController(
            IHtmlToPdfDocumentGenerator htmlToPdfDocumentGenerator,
            IHtmlToPdfAsyncConverter htmlToPdfAsyncConverter)
        {
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
            _htmlToPdfDocumentGenerator = htmlToPdfDocumentGenerator;
            _htmlToPdfAsyncConverter = htmlToPdfAsyncConverter;
        }

        [HttpPost]
        public async Task<IHttpActionResult> Post()
        {
            var doc = _htmlToPdfDocumentGenerator.Generate();
            MemoryStream? stream = null;
            _ = await _htmlToPdfAsyncConverter.ConvertAsync(
                doc,
                length =>
                {
                    stream = _recyclableMemoryStreamManager.GetStream(
                        Guid.NewGuid(),
                        "wkhtmltox",
                        length);
                    return stream;
                },
                HttpContext.Current.Request.GetOwinContext().Request.CallCancelled);
            stream!.Position = 0;
#pragma warning disable CA2000 // Dispose objects before losing scope
            var httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
#pragma warning restore CA2000 // Dispose objects before losing scope
            httpResponseMessage.Content = new ByteArrayContent(stream.ToArray());
            httpResponseMessage.Content.Headers.ContentLength = stream.Length;
            httpResponseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "sample.pdf",
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            return ResponseMessage(httpResponseMessage);
        }
    }
}
