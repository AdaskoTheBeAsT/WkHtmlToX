using System.Web.Http;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.BusinessLogic;
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

            app.Use(async (context, next) =>
            {
                using (AsyncScopedLifestyle.BeginScope(container))
                {
                    await next();
                }
            });

            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            httpConfiguration.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);
            container.RegisterSingleton<IHtmlGenerator, SmallHtmlGenerator>();
            container.RegisterSingleton<IHtmlToPdfDocumentGenerator, HtmlToPdfDocumentGenerator>();
            container.RegisterSingleton<IHtmlToPdfAsyncConverter, SynchronizedPdfConverter>();

            return container;
        }
    }
}
