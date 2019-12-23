using System.Collections.Generic;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Loaders;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace AdaskoTheBeAsT.WkHtmlToX.WebApiCore
{
    public partial class Startup
    {
        private const string LibraryLoaderKey = "LibraryLoader";

        private void ConfigureAppStart(
            IApplicationBuilder app)
        {
            var libFactory = new LibraryLoaderFactory();
            var libraryLoader = libFactory.Create(null);
            libraryLoader.Load();
            app.Properties.Add(new KeyValuePair<string, object>(LibraryLoaderKey, libraryLoader));
        }

        private void ConfigureAppDispose(
            IApplicationBuilder app,
            IHostApplicationLifetime appLifetime)
        {
            appLifetime.ApplicationStopped.Register(() =>
            {
                var libHandle = (ILibraryLoader)app.Properties[LibraryLoaderKey];
                libHandle.Dispose();
            });
        }
    }
}
