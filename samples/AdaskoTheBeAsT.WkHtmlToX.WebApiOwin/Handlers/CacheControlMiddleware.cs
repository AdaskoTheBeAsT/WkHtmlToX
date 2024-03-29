using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

namespace AdaskoTheBeAsT.WkHtmlToX.WebApiOwin.Handlers
{
    public static class CacheControlMiddleware
    {
        public static Task MiddlewareAsync(IOwinContext context, Func<Task> next)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (next is null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (context.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            }

#pragma warning disable CC0031 // Check for null before calling a delegate
            return next();
#pragma warning restore CC0031 // Check for null before calling a delegate
        }

        public static void PreventResponseCaching(this IAppBuilder app)
        {
            app.Use((context, func) => MiddlewareAsync(context, func));
        }
    }
}
