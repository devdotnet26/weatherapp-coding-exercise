namespace WeatherApp.Api.Models;

/// <summary>
/// Represents the weather result for a single date, whether successful or not.
/// </summary>
public class WeatherEntry
{
    /// <summary>Raw string from dates.txt before parsing.</summary>
    public string RawDate { get; set; } = string.Empty;

    /// <summary>ISO 8601 date string (yyyy-MM-dd) after successful parsing. Null if parsing failed.</summary>
    public string? Date { get; set; }

    /// <summary>Minimum temperature in °C for the day.</summary>
    public double? MinTemperatureCelsius { get; set; }

    /// <summary>Maximum temperature in °C for the day.</summary>
    public double? MaxTemperatureCelsius { get; set; }

    /// <summary>Total precipitation in mm for the day.</summary>
    public double? PrecipitationMm { get; set; }

    /// <summary>Indicates whether this entry represents a successful fetch.</summary>
    public bool IsSuccess { get; set; }

    /// <summary>Human-readable error or status message when something went wrong.</summary>
    public string? ErrorMessage { get; set; }
}
