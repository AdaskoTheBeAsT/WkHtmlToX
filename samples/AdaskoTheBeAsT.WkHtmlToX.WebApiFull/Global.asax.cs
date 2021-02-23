using System.Web.Http;

namespace AdaskoTheBeAsT.WkHtmlToX.WebApiFull
{
#pragma warning disable SA1649 // File name should match first type name
#pragma warning disable MA0048 // File name must match type name
    public class WebApiApplication : System.Web.HttpApplication
#pragma warning restore MA0048 // File name must match type name
#pragma warning restore SA1649 // File name should match first type name
    {
#pragma warning disable CA1707 // Identifiers should not contain underscores
        protected void Application_Start()
#pragma warning restore CA1707 // Identifiers should not contain underscores
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            GlobalConfiguration.Configure(SwaggerConfig.Register);
        }
    }
}
