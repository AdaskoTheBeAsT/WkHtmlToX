using System.Web.Http;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Loaders;

namespace AdaskoTheBeAsT.WkHtmlToX.WebApiFull
{
#pragma warning disable SA1649 // File name should match first type name
    public class WebApiApplication : System.Web.HttpApplication
#pragma warning restore SA1649 // File name should match first type name
    {
        private ILibraryLoader _libraryLoader;

#pragma warning disable CA1707 // Identifiers should not contain underscores
        protected void Application_Start()
#pragma warning restore CA1707 // Identifiers should not contain underscores
        {
            var libFactory = new LibraryLoaderFactory();
            _libraryLoader = libFactory.Create(null);
            _libraryLoader.Load();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            GlobalConfiguration.Configure(SwaggerConfig.Register);
        }

#pragma warning disable CA1707 // Identifiers should not contain underscores
        protected void Application_Stop()
#pragma warning restore CA1707 // Identifiers should not contain underscores
        {
            _libraryLoader.Dispose();
        }
    }
}
