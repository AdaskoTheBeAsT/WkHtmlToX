using System;
using System.Net.Http;
using System.Web.Http;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.BusinessLogic;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using AdaskoTheBeAsT.WkHtmlToX.WebApiOwin.Handlers;
using Microsoft.Owin;
using Owin;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;
using SimpleInjector.Lifestyles;

namespace AdaskoTheBeAsT.WkHtmlToX.WebApiOwin
{
    public partial class Startup
    {
        private Container ConfigureIoC(IAppBuilder app, HttpConfiguration httpConfiguration)
        {
            var container = new Container();

            app.Use(async (_, next) =>
            {
                using (AsyncScopedLifestyle.BeginScope(container))
                {
#pragma warning disable CC0031 // Check for null before calling a delegate
#pragma warning disable MA0004 // Use .ConfigureAwait(false)
                    await next();
#pragma warning restore MA0004 // Use .ConfigureAwait(false)
#pragma warning restore CC0031 // Check for null before calling a delegate
                }
            });

            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            container.Register(
                () =>
                    container.GetInstance<CurrentRequest>().Value.GetOwinContext(),
                Lifestyle.Scoped);
            container.RegisterSingleton<IHtmlGenerator, SmallHtmlGenerator>();
            container.RegisterSingleton<IHtmlToPdfDocumentGenerator, HtmlToPdfDocumentGenerator>();
            var configuration = new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, null);
            container.RegisterInstance(configuration);
            container.RegisterSingleton<IWkHtmlToXEngine, WkHtmlToXEngine>();
            container.RegisterSingleton<IPdfConverter, PdfConverter>();
            container.RegisterInitializer<IWkHtmlToXEngine>(e => e.Initialize());
            container.RegisterWebApiControllers(httpConfiguration);

            httpConfiguration.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);
            return container;
        }
    }
}
