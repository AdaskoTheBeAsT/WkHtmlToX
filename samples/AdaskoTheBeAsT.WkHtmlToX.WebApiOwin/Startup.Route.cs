using System.Web.Http;

namespace AdaskoTheBeAsT.WkHtmlToX.WebApiOwin
{
    public partial class Startup
    {
#pragma warning disable CC0091 // Use static method
        public void ConfigureRoute(HttpConfiguration httpConfiguration)
#pragma warning restore CC0091 // Use static method
        {
            // Configure Web API Routes:
            // - Enable Attribute Mapping
            // - Enable Default routes at /api.
            httpConfiguration.MapHttpAttributeRoutes();
            httpConfiguration.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });
        }
    }
}
