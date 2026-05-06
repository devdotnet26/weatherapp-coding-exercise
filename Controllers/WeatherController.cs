using Microsoft.AspNetCore.Mvc;
using WeatherApp.Api.Services;

namespace WeatherApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;
    private readonly ILogger<WeatherController> _logger;

    public WeatherController(IWeatherService weatherService, ILogger<WeatherController> logger)
    {
        _weatherService = weatherService;
        _logger = logger;
    }

    /// <summary>
    /// Returns weather entries for every date in dates.txt.
    /// Invalid dates and fetch failures are included with IsSuccess=false and an ErrorMessage.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        try
        {
            var results = await _weatherService.GetAllWeatherAsync(ct);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET /api/weather");
            return StatusCode(500, new { error = "An unexpected error occurred. See server logs for details." });
        }
    }
}
