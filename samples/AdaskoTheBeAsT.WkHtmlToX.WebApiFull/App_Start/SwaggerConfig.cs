using System;
using System.IO;
using System.Reflection;
using System.Web.Http;
using Swashbuckle.Application;

namespace AdaskoTheBeAsT.WkHtmlToX.WebApiFull
{
    public static class SwaggerConfig
    {
        public static void Register(
            HttpConfiguration config)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var commentsFileName = $"bin\\{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var commentsFile = Path.Combine(baseDirectory, commentsFileName);

            GlobalConfiguration.Configuration
                .EnableSwagger(c =>
                {
                    c.SingleApiVersion("v1", "WebApiFull");
                    c.IncludeXmlComments(commentsFile);
                })
                .EnableSwaggerUi();
        }
    }
}
