using System;
using System.Collections.Generic;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Loaders;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace AdaskoTheBeAsT.WkHtmlToX.WebApi
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
            if (libraryLoader is null)
            {
#pragma warning disable MA0014 // Do not raise System.ApplicationException type
#pragma warning disable S112 // General exceptions should never be thrown
                throw new ApplicationException("Library loader cannot be null");
#pragma warning restore S112 // General exceptions should never be thrown
#pragma warning restore MA0014 // Do not raise System.ApplicationException type
            }

            libraryLoader.Load();
            app.Properties.Add(new KeyValuePair<string, object?>(LibraryLoaderKey, libraryLoader));
        }

        private void ConfigureAppDispose(
            IApplicationBuilder app,
            IHostApplicationLifetime appLifetime)
        {
            var handle = app.Properties[LibraryLoaderKey] as ILibraryLoader;
            if (handle is null)
            {
#pragma warning disable MA0014 // Do not raise System.ApplicationException type
#pragma warning disable S112 // General exceptions should never be thrown
                throw new ApplicationException("Library handler needs to be properly initialized");
#pragma warning restore S112 // General exceptions should never be thrown
#pragma warning restore MA0014 // Do not raise System.ApplicationException type
            }
#pragma warning disable IDISP004 // Don't ignore created IDisposable.
            appLifetime.ApplicationStopped.Register(() =>
            {
                var libHandle = handle;
                libHandle.Dispose();
            });
#pragma warning restore IDISP004 // Don't ignore created IDisposable.
        }
    }
}
