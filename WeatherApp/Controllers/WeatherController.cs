using Microsoft.AspNetCore.Mvc;
using WeatherApp.Models;
using WeatherApp.Services;

namespace WeatherApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherAggregatorService _aggregatorService;
    private readonly ILogger<WeatherController> _logger;

    public WeatherController(
        IWeatherAggregatorService aggregatorService,
        ILogger<WeatherController> logger)
    {
        _aggregatorService = aggregatorService;
        _logger = logger;
    }

    /// <summary>
    /// Returns one entry per line in dates.txt: normalized date, min/max temperature,
    /// precipitation, and a status/error message for any date that failed or was invalid.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WeatherRecord>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<WeatherRecord>>> Get(CancellationToken cancellationToken)
    {
        try
        {
            var records = await _aggregatorService.GetWeatherAsync(cancellationToken);
            return Ok(records);
        }
        catch (Exception ex)
        {
            // The aggregator/service layer already converts per-date failures into
            // WeatherRecord entries; this catch is a last-resort guard against anything
            // unexpected (e.g. dates.txt path misconfigured) so the endpoint never 500s
            // without at least logging why.
            _logger.LogError(ex, "Unexpected failure while building the weather response.");
            return Problem(
                title: "Failed to retrieve weather data.",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
