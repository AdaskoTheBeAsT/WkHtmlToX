#nullable enable
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;

namespace AdaskoTheBeAsT.WkHtmlToX.WebApiOwin.Handlers
{
    public sealed class ExceptionSpecialFilterAttribute
        : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Exception is HttpResponseException exception)
            {
                actionExecutedContext.Response = exception.Response;
                return;
            }

            var responseContent = new ErrorResponse(actionExecutedContext.Exception);
            var status = HttpStatusCode.InternalServerError;
            if (actionExecutedContext.Exception is KeyNotFoundException)
            {
                status = HttpStatusCode.NotFound;
            }

            actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(status, responseContent);
        }

        private sealed class ErrorResponse
        {
            public ErrorResponse(Exception ex)
            {
                Error = new ErrorDescription
                {
                    Message = ex.Message,
                    InnerException = ex.InnerException?.Message,
                };
            }

            public ErrorDescription Error { get; }

            public class ErrorDescription
            {
                public string? Message { get; set; } = string.Empty;

                public string? InnerException { get; set; } = string.Empty;
            }
        }
    }
}
