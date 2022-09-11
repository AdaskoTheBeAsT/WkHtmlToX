using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using Swashbuckle.Application;

namespace AdaskoTheBeAsT.WkHtmlToX.WebApiOwin
{
    public partial class Startup
    {
        private void ConfigureSwagger(HttpConfiguration httpConfiguration)
        {
            httpConfiguration.EnableSwagger(
                c =>
                {
                    var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    var commentsFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var commentsFile = Path.Combine(baseDirectory, "bin", commentsFileName);
                    c.SingleApiVersion("v1", "owin api");
                    c.PrettyPrint();
                    c.IncludeXmlComments(commentsFile);
                    c.RootUrl(
                        req => string.Concat(
                                req.RequestUri.GetLeftPart(UriPartial.Authority),
                                req.GetRequestContext().VirtualPathRoot.TrimEnd('/')));
                }).EnableSwaggerUi();
        }
    }
}
