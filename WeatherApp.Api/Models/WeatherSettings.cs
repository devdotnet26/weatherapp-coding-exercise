namespace WeatherApp.Api.Models;

/// <summary>
/// Strongly-typed configuration bound from appsettings.json → WeatherSettings section.
/// </summary>
public class WeatherSettings
{
    public double Latitude { get; set; } = 32.78;
    public double Longitude { get; set; } = -96.8;
    public string Timezone { get; set; } = "America/Chicago";
    public string OpenMeteoBaseUrl { get; set; } = "https://archive-api.open-meteo.com/v1/archive";
    public string WeatherDataFolder { get; set; } = "weather-data";
    public string DatesFile { get; set; } = "dates.txt";
}
