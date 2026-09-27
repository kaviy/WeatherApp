using WeatherApp.Models;

namespace WeatherApp.Services;

/// <summary>
/// Orchestrates the end-to-end flow: read dates.txt, parse each line, and for every
/// valid date either return the cached result or fetch it from the weather API and
/// cache it. This is the only class that coordinates the other, single-purpose
/// services, keeping each of them simple and independently testable.
/// </summary>
public interface IWeatherAggregatorService
{
    Task<IReadOnlyList<WeatherRecord>> GetWeatherAsync(CancellationToken cancellationToken = default);
}
