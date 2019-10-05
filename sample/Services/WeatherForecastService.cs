using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Diagnostics;
using System.Composition.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace sample.Services
{
    public interface IWeatherForcastService
    {
        IEnumerable<WeatherForecast> GetForecasts();
    }

    [Export(typeof(IWeatherForcastService)), Scoped]
    public class WeatherForcastService : IWeatherForcastService, IDisposable
    {
        private Guid _guid = Guid.NewGuid();

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public ILogger<WeatherForcastService> Logger { get; }

        [ImportingConstructor]
        public WeatherForcastService(ILogger<WeatherForcastService> logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Logger.LogInformation($"WeatherForcastService {_guid} was created");
        }

        public IEnumerable<WeatherForecast> GetForecasts()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        public void Dispose()
        {
            Logger.LogInformation($"WeatherForcastService {_guid} was disposed");
        }

        public override string ToString() => _guid.ToString();
    }
}