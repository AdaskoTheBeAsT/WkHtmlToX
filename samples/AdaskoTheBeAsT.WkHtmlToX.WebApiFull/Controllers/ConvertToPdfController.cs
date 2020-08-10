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
        public HttpResponseMessage Post()
        {
            var doc = _htmlToPdfDocumentGenerator.Generate();
            MemoryStream? stream = null;
            _ = _htmlToPdfAsyncConverter.ConvertAsync(
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
                CancellationToken.None).GetAwaiter().GetResult();
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
