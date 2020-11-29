using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleInjector;

namespace AdaskoTheBeAsT.WkHtmlToX.WebApi
{
    public sealed partial class Startup
        : IDisposable
    {
        private readonly Container _container = new Container();

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            ConfigureServicesSwagger(services);
            ConfigureServicesIoC(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            IHostApplicationLifetime appLifetime)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (appLifetime is null)
            {
                throw new ArgumentNullException(nameof(appLifetime));
            }

            ConfigureIoC(app);
            ConfigureAppStart(app);
            ConfigureAppDispose(app, appLifetime);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseStaticFiles();

            ConfigureSwagger(app);

            app.UseEndpoints(endpoints => endpoints.MapControllers());
            _container.Verify();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _container?.Dispose();
            }
        }
    }
}
