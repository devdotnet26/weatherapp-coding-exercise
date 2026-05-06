using System.Globalization;

namespace WeatherApp.Api.Services;

/// <summary>
/// Parses date strings in multiple formats and validates calendar correctness.
/// Supported formats:
///   MM/dd/yyyy        e.g. 02/27/2021
///   MMMM d, yyyy      e.g. June 2, 2022
///   MMM-dd-yyyy       e.g. Jul-13-2020
///   MMMM dd, yyyy     e.g. April 31, 2022 (caught as invalid)
/// </summary>
public class DateParserService : IDateParserService
{
    // We use ParseExact with these formats in order so the first match wins.
    // CultureInfo.InvariantCulture ensures month names are English regardless of server locale.
    private static readonly string[] SupportedFormats =
    [
        "MM/dd/yyyy",       // 02/27/2021
        "MMMM d, yyyy",     // June 2, 2022
        "MMMM dd, yyyy",    // April 31, 2022
        "MMM-dd-yyyy",      // Jul-13-2020
        "MMM-d-yyyy",       // Jul-3-2020 (single-digit day variant)
    ];

    public bool TryParse(string rawInput, out string? isoDate, out string? errorMessage)
    {
        isoDate = null;
        errorMessage = null;

        var trimmed = rawInput.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            errorMessage = "Date string is empty.";
            return false;
        }

        // DateTime.ParseExact with DateTimeStyles.None rejects logically impossible dates
        // (e.g. April 31) because .NET does NOT silently roll them over — it throws.
        foreach (var fmt in SupportedFormats)
        {
            if (DateTime.TryParseExact(
                    trimmed,
                    fmt,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsed))
            {
                isoDate = parsed.ToString("yyyy-MM-dd");
                return true;
            }
        }

        // None of the formats matched. Give a clear message.
        errorMessage = $"Unable to parse '{trimmed}' into a valid date. " +
                       "Supported formats: MM/dd/yyyy, MMMM d, yyyy, MMM-dd-yyyy.";
        return false;
    }
}
