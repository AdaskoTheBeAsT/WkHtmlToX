using System;
using System.Web.Http;
using AdaskoTheBeAsT.WkHtmlToX.WebApiOwin.Handlers;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Owin;

[assembly: OwinStartup(typeof(AdaskoTheBeAsT.WkHtmlToX.WebApiOwin.Startup))]

namespace AdaskoTheBeAsT.WkHtmlToX.WebApiOwin
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

#pragma warning disable CA2000 // Dispose objects before losing scope
#pragma warning disable IDISP001 // Dispose created.
            var httpConfiguration = new HttpConfiguration();
#pragma warning restore IDISP001 // Dispose created.
#pragma warning restore CA2000 // Dispose objects before losing scope

            ConfigureSwagger(httpConfiguration);

            // Configure Web API Routes:
            // - Enable Attribute Mapping
            // - Enable Default routes at /api.
            httpConfiguration.MapHttpAttributeRoutes();
            httpConfiguration.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });

#pragma warning disable CA2000 // Dispose objects before losing scope
#pragma warning disable IDISP001 // Dispose created.
            var container = ConfigureIoC(app, httpConfiguration);
#pragma warning restore IDISP001 // Dispose created.
#pragma warning restore CA2000 // Dispose objects before losing scope
            container.Verify();
            app.PreventResponseCaching();
            app.UseWebApi(httpConfiguration);
            httpConfiguration.EnsureInitialized();

            app.UseStageMarker(PipelineStage.MapHandler);
        }
    }
}
