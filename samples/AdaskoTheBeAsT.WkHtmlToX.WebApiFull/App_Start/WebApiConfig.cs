using System;
using System.Web.Http;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.BusinessLogic;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;
using SimpleInjector.Lifestyles;

namespace AdaskoTheBeAsT.WkHtmlToX.WebApiFull
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            // Web API configuration and services
#pragma warning disable CA2000 // Dispose objects before losing scope
#pragma warning disable IDISP001 // Dispose created.
            var container = new Container();
#pragma warning restore IDISP001 // Dispose created.
#pragma warning restore CA2000 // Dispose objects before losing scope
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            // Register your types, for instance using the scoped lifestyle:
            container.RegisterSingleton<IHtmlGenerator, SmallHtmlGenerator>();
            container.RegisterSingleton<IHtmlToPdfDocumentGenerator, HtmlToPdfDocumentGenerator>();
            container.RegisterSingleton<IHtmlToPdfAsyncConverter, SynchronizedPdfConverter>();

            // This is an extension method from the integration package.
            container.RegisterWebApiControllers(GlobalConfiguration.Configuration);

            container.Verify();

            GlobalConfiguration.Configuration.DependencyResolver =
                new SimpleInjectorWebApiDependencyResolver(container);

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });
        }
    }
}
