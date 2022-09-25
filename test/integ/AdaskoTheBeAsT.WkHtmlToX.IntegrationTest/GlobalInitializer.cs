using System;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using TechTalk.SpecFlow;

namespace AdaskoTheBeAsT.WkHtmlToX.IntegrationTest
{
    [Binding]
    public static class GlobalInitializer
    {
        public static IWkHtmlToXEngine? Engine { get; private set; }

        [BeforeTestRun]
        public static void Initialize()
        {
            var configuration = new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, runtimeIdentifier: null);
#pragma warning disable IDISP007 // Don't dispose injected
            Engine?.Dispose();
#pragma warning restore IDISP007 // Don't dispose injected
            Engine = new WkHtmlToXEngine(configuration);
            Engine.Initialize();
        }
    }
}
