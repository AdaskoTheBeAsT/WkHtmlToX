using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.BusinessLogic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IO;

namespace AdaskoTheBeAsT.WkHtmlToX.WebApiCore.Controllers
{
    [Produces("application/pdf")]
    [ApiController]
    [Route("api/[controller]")]
    public class ConvertToPdfController
        : ControllerBase
    {
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHtmlToPdfDocumentGenerator _htmlToPdfDocumentGenerator;
        private readonly IHtmlToPdfAsyncConverter _htmlToPdfAsyncConverter;

        public ConvertToPdfController(
            IHttpContextAccessor httpContextAccessor,
            IHtmlToPdfDocumentGenerator htmlToPdfDocumentGenerator,
            IHtmlToPdfAsyncConverter htmlToPdfAsyncConverter)
        {
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
            _httpContextAccessor = httpContextAccessor;
            _htmlToPdfDocumentGenerator = htmlToPdfDocumentGenerator;
            _htmlToPdfAsyncConverter = htmlToPdfAsyncConverter;
        }

        [HttpPost]
#pragma warning disable SEC0019 // Missing AntiForgeryToken Attribute
#pragma warning disable SEC0120 // Missing Authorization Attribute
        public async Task<IActionResult> Convert()
#pragma warning restore SEC0120 // Missing Authorization Attribute
#pragma warning restore SEC0019 // Missing AntiForgeryToken Attribute
        {
            var doc = _htmlToPdfDocumentGenerator.Generate();
            Stream? stream = null;
            var converted = await _htmlToPdfAsyncConverter.ConvertAsync(
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
                _httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None);
            stream!.Position = 0;
            if (converted)
            {
                var result = new FileStreamResult(stream, "application/pdf")
                {
                    FileDownloadName = "sample.pdf",
                };

                return result;
            }

            return BadRequest();
        }
    }
}
