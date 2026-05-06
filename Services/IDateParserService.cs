namespace WeatherApp.Api.Services;

public interface IDateParserService
{
    /// <summary>
    /// Attempts to parse a raw date string in any supported format into ISO 8601 (yyyy-MM-dd).
    /// </summary>
    /// <param name="rawInput">The raw date string from dates.txt.</param>
    /// <param name="isoDate">The normalized ISO date if parsing succeeded; null otherwise.</param>
    /// <param name="errorMessage">A descriptive error if parsing failed; null otherwise.</param>
    /// <returns>True if parsing and validation succeeded.</returns>
    bool TryParse(string rawInput, out string? isoDate, out string? errorMessage);
}
