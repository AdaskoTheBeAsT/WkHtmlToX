using System;
using System.IO;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using SerilogWeb.Classic;

namespace AdaskoTheBeAsT.WkHtmlToX.WebApiOwin
{
    public partial class Startup
    {
#pragma warning disable CC0091 // Use static method
        public void ConfigureLogger(HttpConfiguration config)
#pragma warning restore CC0091 // Use static method
        {
            // Use Serilog for logging
            // More information can be found here https://github.com/serilog/serilog/wiki/Getting-Started
#pragma warning disable SCS0018
            var f = new FileInfo(Assembly.GetExecutingAssembly().Location);
#pragma warning restore SCS0018

            var name = f.Name.Replace(f.Extension, string.Empty);

            // By default log file is located in 'C:\Users\<username>\AppData\Roaming\Logs' folder and named as the current assembly name
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(Environment.ExpandEnvironmentVariables($"%AppData%/Logs/{name}.txt"), rollingInterval: RollingInterval.Day)
                .ReadFrom.AppSettings()

                // Enrich with SerilogWeb.Classic (https://github.com/serilog-web/classic)
                .Enrich.WithHttpRequestUrl()
                .Enrich.WithHttpRequestType()

                .Enrich.WithExceptionDetails()

                .CreateLogger();

            // By default we don't want to see all HTTP requests in log file, but you can change this by adjusting this setting
            // Additional information can be found here https://github.com/serilog-web/classic
            // All requests will
            SerilogWebClassic.Configure(
                cfg =>
                    cfg.LogAtLevel(LogEventLevel.Debug));

            config.Services.Replace(typeof(IExceptionLogger), new Handlers.ExceptionLogger());
        }
    }
}
