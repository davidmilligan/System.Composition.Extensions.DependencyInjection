using System.Composition;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace sample.Services
{
    public class CustomMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, IWeatherForcastService service, ILogger<CustomMiddleware> logger)
        {
            logger.LogInformation($"Middleware service {service}");
            await _next(httpContext);
        }
    }
}