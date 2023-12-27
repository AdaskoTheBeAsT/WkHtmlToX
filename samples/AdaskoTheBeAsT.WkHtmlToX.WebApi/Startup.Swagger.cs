using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace AdaskoTheBeAsT.WkHtmlToX.WebApi
{
    public partial class Startup
    {
        private const string SwaggerEndpoint = "/swagger/v1/swagger.json";
        private const string RedocEndpoint = "/swagger/v1/swagger.json";
        private const string RedocRoutePrefix = "api-docs";

#pragma warning disable CC0091 // Use static method
        public void ConfigureServicesSwagger(IServiceCollection services)
#pragma warning restore CC0091 // Use static method
        {
            // Register the Swagger generator, defining one or more Swagger documents
            _ = services.AddSwaggerGen(c =>
              {
                  c.SwaggerDoc("v1", new OpenApiInfo { Title = nameof(WebApi.Startup), Version = "v1" });

                  // Set the comments path for the Swagger JSON and UI.
                  var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                  var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                  c.IncludeXmlComments(xmlPath);
              });
        }

        public void ConfigureSwagger(
            IApplicationBuilder app)
        {
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(
                    SwaggerEndpoint,
                    $"{GetType().FullName?.Replace(".Startup", string.Empty, StringComparison.OrdinalIgnoreCase)}");
            });

            app.UseReDoc(c =>
            {
                c.SpecUrl = RedocEndpoint;
                c.RoutePrefix = RedocRoutePrefix;
            });
        }
    }
}
