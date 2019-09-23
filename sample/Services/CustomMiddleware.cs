using System.Composition;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace sample.Services
{
    public class CustomMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, IWeatherForcastService service)
        {
            Trace.WriteLine($"Middleware service {service}");
            await _next(httpContext);
        }
    }
}