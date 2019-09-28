using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using sample.Services;

namespace sample.Controllers
{
    [Export]
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private ILogger _logger;

        [Import]
        public ILoggerFactory LoggerFactory { get; set; }

        public ILogger Logger => _logger ??= LoggerFactory.CreateLogger<WeatherForecastController>();

        [Import]
        public IWeatherForcastService WeatherForcastService { get; set; }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            Logger.LogInformation(WeatherForcastService.ToString());
            return WeatherForcastService.GetForecasts();
        }
    }
}
