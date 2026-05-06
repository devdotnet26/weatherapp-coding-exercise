using System.Text.Json.Serialization;

namespace WeatherApp.Api.Models;

/// <summary>
/// Strongly-typed representation of the Open-Meteo /v1/archive JSON response.
/// </summary>
public class OpenMeteoResponse
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("daily")]
    public DailyData? Daily { get; set; }
}

public class DailyData
{
    [JsonPropertyName("time")]
    public List<string>? Time { get; set; }

    [JsonPropertyName("temperature_2m_max")]
    public List<double?>? Temperature2mMax { get; set; }

    [JsonPropertyName("temperature_2m_min")]
    public List<double?>? Temperature2mMin { get; set; }

    [JsonPropertyName("precipitation_sum")]
    public List<double?>? PrecipitationSum { get; set; }
}
