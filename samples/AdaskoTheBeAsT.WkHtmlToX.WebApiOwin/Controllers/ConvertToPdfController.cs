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
        private readonly IPdfConverter _pdfConverter;

        public ConvertToPdfController(
            IHtmlToPdfDocumentGenerator htmlToPdfDocumentGenerator,
            IPdfConverter pdfConverter)
        {
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
            _htmlToPdfDocumentGenerator = htmlToPdfDocumentGenerator;
            _pdfConverter = pdfConverter;
        }

        [HttpPost]
        public async Task<IHttpActionResult> Post()
        {
            var doc = _htmlToPdfDocumentGenerator.Generate();
            MemoryStream? stream = null;
            _ = await _pdfConverter.ConvertAsync(
                doc,
                length =>
                {
#pragma warning disable IDISP003 // Dispose previous before re-assigning.
                    stream = _recyclableMemoryStreamManager.GetStream(
                        Guid.NewGuid(),
                        "wkhtmltox",
                        length);
#pragma warning restore IDISP003 // Dispose previous before re-assigning.
                    return stream;
                },
                HttpContext.Current.Request.GetOwinContext().Request.CallCancelled).ConfigureAwait(false);
            stream!.Position = 0;
#pragma warning disable CA2000 // Dispose objects before losing scope
#pragma warning disable IDISP001 // Dispose created.
            var httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
#pragma warning restore IDISP001 // Dispose created.
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
