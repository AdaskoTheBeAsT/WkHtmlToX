using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AdaskoTheBeAsT.WkHtmlToX.WebApiOwin.Handlers
{
    public class CurrentRequestHandler
        : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            using (var scope = request.GetDependencyScope())
            {
                var currentRequest = (CurrentRequest)scope.GetService(typeof(CurrentRequest));
                currentRequest.Value = request;
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
