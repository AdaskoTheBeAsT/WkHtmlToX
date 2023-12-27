using System;
using System.Net.Http.Formatting;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AdaskoTheBeAsT.WkHtmlToX.WebApiOwin
{
    public partial class Startup
    {
#pragma warning disable CC0091 // Use static method
        public void ConfigureFormatter(HttpConfiguration config)
#pragma warning restore CC0091 // Use static method
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            config.Formatters.Clear();

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Include,
#if DEBUG
                Formatting = Formatting.Indented,
#else
                Formatting = Formatting.None,
#endif
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };

            JsonConvert.DefaultSettings = () => settings;

            config.Formatters.Add(new JsonMediaTypeFormatter());

            config.Formatters.JsonFormatter.SerializerSettings = settings;

            config.Formatters.JsonFormatter.UseDataContractJsonSerializer = false;
        }
    }
}
