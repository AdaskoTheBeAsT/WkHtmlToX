#nullable enable
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Web.Http;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.BusinessLogic;
using Microsoft.IO;

namespace AdaskoTheBeAsT.WkHtmlToX.WebApiFull.Controllers
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
        public HttpResponseMessage Post()
        {
            var doc = _htmlToPdfDocumentGenerator.Generate();
            MemoryStream? stream = null;

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
#pragma warning disable IDISP003 // Dispose previous before re-assigning.
            _ = _pdfConverter.ConvertAsync(
                doc,
                length =>
                {
                    stream = _recyclableMemoryStreamManager.GetStream(
                        Guid.NewGuid(),
                        "wkhtmltox",
                        length);

                    return stream;
                },
                CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore IDISP003 // Dispose previous before re-assigning.
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
            stream!.Position = 0;
            var httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
            httpResponseMessage.Content = new ByteArrayContent(stream.ToArray());
            httpResponseMessage.Content.Headers.ContentLength = stream.Length;
            httpResponseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "sample.pdf",
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            return httpResponseMessage;
        }
    }
}
