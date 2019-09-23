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
        [Import]
        public IWeatherForcastService WeatherForcastService { get; set; }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            Trace.WriteLine(WeatherForcastService.ToString());
            return WeatherForcastService.GetForecasts();
        }
    }
}
