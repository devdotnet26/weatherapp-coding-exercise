using System.Text.Json;
using Microsoft.Extensions.Options;
using WeatherApp.Api.Models;

namespace WeatherApp.Api.Services;

/// <summary>
/// Orchestrates reading dates.txt → parsing → caching → Open-Meteo fetch → result assembly.
/// </summary>
public class WeatherService : IWeatherService
{
    private readonly IDateParserService _dateParser;
    private readonly HttpClient _httpClient;
    private readonly WeatherSettings _settings;
    private readonly ILogger<WeatherService> _logger;

    // Reuse JsonSerializerOptions for efficiency
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public WeatherService(
        IDateParserService dateParser,
        HttpClient httpClient,
        IOptions<WeatherSettings> settings,
        ILogger<WeatherService> logger)
    {
        _dateParser = dateParser;
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<WeatherEntry>> GetAllWeatherAsync(CancellationToken ct = default)
    {
        var datesFilePath = ResolveRelativePath(_settings.DatesFile);
        var weatherFolder = ResolveRelativePath(_settings.WeatherDataFolder);

        // Ensure storage folder exists
        Directory.CreateDirectory(weatherFolder);

        if (!File.Exists(datesFilePath))
        {
            _logger.LogError("dates.txt not found at {Path}", datesFilePath);
            return [];
        }

        var lines = await File.ReadAllLinesAsync(datesFilePath, ct);
        var results = new List<WeatherEntry>();

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var entry = new WeatherEntry { RawDate = line.Trim() };

            // --- Step 1: Parse the date ---
            if (!_dateParser.TryParse(line, out var isoDate, out var parseError))
            {
                _logger.LogWarning("Invalid date '{Line}': {Error}", line, parseError);
                entry.IsSuccess = false;
                entry.ErrorMessage = parseError;
                results.Add(entry);
                continue;
            }

            entry.Date = isoDate!;

            // --- Step 2: Check cache ---
            var cacheFile = Path.Combine(weatherFolder, $"{isoDate}.json");
            if (File.Exists(cacheFile))
            {
                _logger.LogInformation("Cache hit for {Date}", isoDate);
                var cached = await LoadFromCacheAsync(cacheFile, ct);
                if (cached is not null)
                {
                    cached.RawDate = entry.RawDate; // keep original raw string
                    results.Add(cached);
                    continue;
                }
                // Cache file was corrupt — fall through to re-fetch
                _logger.LogWarning("Cache file for {Date} was unreadable; re-fetching.", isoDate);
            }

            // --- Step 3: Fetch from Open-Meteo ---
            var fetched = await FetchFromApiAsync(isoDate!, ct);
            fetched.RawDate = entry.RawDate;
            fetched.Date = isoDate;

            // --- Step 4: Persist result (only on success) ---
            if (fetched.IsSuccess)
            {
                await SaveToCacheAsync(cacheFile, fetched, ct);
            }

            results.Add(fetched);
        }

        return results;
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private async Task<WeatherEntry> FetchFromApiAsync(string isoDate, CancellationToken ct)
    {
        var url = BuildUrl(isoDate);
        _logger.LogInformation("Fetching weather for {Date} from Open-Meteo", isoDate);

        try
        {
            var response = await _httpClient.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            var meteo = JsonSerializer.Deserialize<OpenMeteoResponse>(json, JsonOpts);

            if (meteo?.Daily?.Time is null || meteo.Daily.Time.Count == 0)
            {
                return ErrorEntry($"Open-Meteo returned no daily data for {isoDate}.");
            }

            // Find the index matching our date (API may return a range; we ask for one day)
            var idx = meteo.Daily.Time.IndexOf(isoDate);
            if (idx < 0)
            {
                return ErrorEntry($"Response did not include data for {isoDate}.");
            }

            return new WeatherEntry
            {
                IsSuccess = true,
                MinTemperatureCelsius = meteo.Daily.Temperature2mMin?[idx],
                MaxTemperatureCelsius = meteo.Daily.Temperature2mMax?[idx],
                PrecipitationMm       = meteo.Daily.PrecipitationSum?[idx],
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error fetching weather for {Date}", isoDate);
            return ErrorEntry($"Network error: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Request for {Date} was cancelled or timed out.", isoDate);
            return ErrorEntry("Request timed out.");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parse error for {Date}", isoDate);
            return ErrorEntry($"Failed to parse API response: {ex.Message}");
        }
    }

    private string BuildUrl(string isoDate)
    {
        var s = _settings;
        return $"{s.OpenMeteoBaseUrl}" +
               $"?latitude={s.Latitude}" +
               $"&longitude={s.Longitude}" +
               $"&start_date={isoDate}" +
               $"&end_date={isoDate}" +
               $"&daily=temperature_2m_max,temperature_2m_min,precipitation_sum" +
               $"&timezone={Uri.EscapeDataString(s.Timezone)}";
    }

    private static WeatherEntry ErrorEntry(string message) =>
        new() { IsSuccess = false, ErrorMessage = message };

    private static async Task<WeatherEntry?> LoadFromCacheAsync(string path, CancellationToken ct)
    {
        try
        {
            var json = await File.ReadAllTextAsync(path, ct);
            return JsonSerializer.Deserialize<WeatherEntry>(json, JsonOpts);
        }
        catch
        {
            return null;
        }
    }

    private static async Task SaveToCacheAsync(string path, WeatherEntry entry, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(entry, JsonOpts);
        await File.WriteAllTextAsync(path, json, ct);
    }

    /// <summary>
    /// Resolves a path relative to the application's base directory so the app
    /// works regardless of the working directory from which it is launched.
    /// </summary>
    private static string ResolveRelativePath(string relativePath)
    {
        if (Path.IsPathRooted(relativePath))
            return relativePath;

        return Path.Combine(AppContext.BaseDirectory, relativePath);
    }
}
