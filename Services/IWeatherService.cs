using WeatherApp.Api.Models;

namespace WeatherApp.Api.Services;

public interface IWeatherService
{
    /// <summary>
    /// Reads dates.txt, parses each line, fetches weather from Open-Meteo for valid dates
    /// (using cached JSON when available), and returns one WeatherEntry per line.
    /// </summary>
    Task<IReadOnlyList<WeatherEntry>> GetAllWeatherAsync(CancellationToken ct = default);
}
