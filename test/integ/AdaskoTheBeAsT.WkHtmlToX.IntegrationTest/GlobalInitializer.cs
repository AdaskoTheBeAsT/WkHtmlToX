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
            var configuration = new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, null);
            Engine?.Dispose();
            Engine = new WkHtmlToXEngine(configuration);
            Engine.Initialize();
        }
    }
}
