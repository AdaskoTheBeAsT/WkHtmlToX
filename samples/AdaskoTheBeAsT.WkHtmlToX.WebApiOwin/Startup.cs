using System;
using System.IO;
using System.Web;
using System.Web.Http;
using AdaskoTheBeAsT.WkHtmlToX.WebApiOwin.Handlers;
using DotNetEnv;
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

            var envFilePath = Path.Combine(HttpRuntime.AppDomainAppPath, ".env");
            if (File.Exists(envFilePath))
            {
                Env.Load(envFilePath);
            }

            var corsOptions = ConfigureCors(Settings.AllowedOrigins);
            app.UseCors(corsOptions);

#pragma warning disable CA2000 // Dispose objects before losing scope
#pragma warning disable IDISP001 // Dispose created.
            var httpConfiguration = new HttpConfiguration();
#pragma warning restore IDISP001 // Dispose created.
#pragma warning restore CA2000 // Dispose objects before losing scope

#pragma warning disable CA2000 // Dispose objects before losing scope
#pragma warning disable CC0022 // Should dispose object
            httpConfiguration.MessageHandlers.Insert(0, new CurrentRequestHandler());
#pragma warning restore CC0022 // Should dispose object
#pragma warning restore CA2000 // Dispose objects before losing scope

            httpConfiguration.Filters.Add(new ExceptionSpecialFilterAttribute());

            ConfigureFormatter(httpConfiguration);
            ConfigureSwagger(httpConfiguration);
            ConfigureRoute(httpConfiguration);
            ConfigureLogger(httpConfiguration);

#pragma warning disable CA2000 // Dispose objects before losing scope
#pragma warning disable IDISP001 // Dispose created.
            var container = ConfigureIoC(app, httpConfiguration);
#pragma warning restore IDISP001 // Dispose created.
#pragma warning restore CA2000 // Dispose objects before losing scope
            container.Verify();
            app.PreventResponseCaching();
            UseAuthentication(app);
            app.UseWebApi(httpConfiguration);
            httpConfiguration.EnsureInitialized();

            app.UseStageMarker(PipelineStage.MapHandler);
        }
    }
}
