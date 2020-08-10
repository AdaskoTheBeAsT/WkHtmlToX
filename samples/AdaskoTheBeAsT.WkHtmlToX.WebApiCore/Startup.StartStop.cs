using System;
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
#pragma warning disable IDISP001
            var libraryLoader = libFactory.Create((int)Environment.OSVersion.Platform, null);
#pragma warning restore IDISP001
            libraryLoader.Load();
            app.Properties.Add(new KeyValuePair<string, object>(LibraryLoaderKey, libraryLoader));
        }

        private void ConfigureAppDispose(
            IApplicationBuilder app,
            IHostApplicationLifetime appLifetime)
        {
#pragma warning disable IDISP004 // Don't ignore created IDisposable.
            appLifetime.ApplicationStopped.Register(() =>
            {
                var libHandle = (ILibraryLoader)app.Properties[LibraryLoaderKey];
                libHandle.Dispose();
            });
#pragma warning restore IDISP004 // Don't ignore created IDisposable.
        }
    }
}
