using System;
using System.Collections.Generic;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Loaders;
using Microsoft.Owin.BuilderProperties;
using Owin;

namespace AdaskoTheBeAsT.WkHtmlToX.WebApiOwin
{
    public partial class Startup
    {
        private const string LibraryLoaderKey = "LibraryLoader";

        private void ConfigureAppStart(IAppBuilder app)
        {
            var libFactory = new LibraryLoaderFactory();
#pragma warning disable IDISP001 // Dispose created.
            var libraryLoader = libFactory.Create((int)Environment.OSVersion.Platform, null);
#pragma warning restore IDISP001 // Dispose created.
            libraryLoader.Load();
            app.Properties.Add(new KeyValuePair<string, object>(LibraryLoaderKey, libraryLoader));
        }

        private void ConfigureAppDispose(IAppBuilder app)
        {
#pragma warning disable IDISP004 // Don't ignore created IDisposable.
            new AppProperties(app.Properties).OnAppDisposing.Register(() =>
            {
                var libHandle = (ILibraryLoader)app.Properties[LibraryLoaderKey];
                libHandle.Dispose();
            });
#pragma warning restore IDISP004 // Don't ignore created IDisposable.
        }
    }
}
