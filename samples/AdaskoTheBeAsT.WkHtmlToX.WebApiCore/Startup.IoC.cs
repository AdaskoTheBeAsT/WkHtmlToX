using System;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.BusinessLogic;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace AdaskoTheBeAsT.WkHtmlToX.WebApiCore
{
    public partial class Startup
    {
        public void ConfigureServicesIoC(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSimpleInjector(
                _container,
                options =>
                {
                    // AddAspNetCore() wraps web requests in a Simple Injector scope.
                    options.AddAspNetCore()

                        // Ensure activation of a specific framework type to be created by
                        // Simple Injector instead of the built-in configuration system.
                        .AddControllerActivation();
                });

            InitializeContainer();
        }

        public void ConfigureIoC(IApplicationBuilder app)
        {
            // UseSimpleInjector() enables framework services to be injected into
            // application components, resolved by Simple Injector.
            app.UseSimpleInjector(_container);
        }

        private void InitializeContainer()
        {
            _container.RegisterSingleton<IHtmlGenerator, SmallHtmlGenerator>();
            _container.RegisterSingleton<IHtmlToPdfDocumentGenerator, HtmlToPdfDocumentGenerator>();
            var configuration = new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, null);
            _container.RegisterInstance(configuration);
            _container.RegisterSingleton<IWkHtmlToXEngine, WkHtmlToXEngine>();
            _container.RegisterSingleton<IPdfConverter, PdfConverter>();
            _container.RegisterInitializer<IWkHtmlToXEngine>(e => e.Initialize());
        }
    }
}
