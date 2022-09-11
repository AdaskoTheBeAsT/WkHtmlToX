using System.Linq;
using System.Threading.Tasks;
using System.Web.Cors;
using Microsoft.Owin.Cors;

namespace AdaskoTheBeAsT.WkHtmlToX.WebApiOwin
{
    public partial class Startup
    {
        public CorsOptions ConfigureCors(string origins)
        {
            if (string.IsNullOrWhiteSpace(origins))
            {
                return CorsOptions.AllowAll;
            }

            var corsPolicy = new CorsPolicy
            {
                AllowAnyMethod = true,
                AllowAnyHeader = true,
                SupportsCredentials = true,
                PreflightMaxAge = 86400,
            };

            corsPolicy.Headers.Add("Authorization");

            // StringSplitOptions.RemoveEmptyEntries doesn't remove whitespaces.
            origins.Split(';')
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList()
                .ForEach(origin => corsPolicy.Origins.Add(origin));

            if (corsPolicy.Origins.Count == 0)
            {
                return CorsOptions.AllowAll;
            }

            return new CorsOptions
            {
                PolicyProvider = new CorsPolicyProvider
                {
                    PolicyResolver = _ => Task.FromResult(corsPolicy),
                },
            };
        }
    }
}
