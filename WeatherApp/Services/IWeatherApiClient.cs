using WeatherApp.Models;

namespace WeatherApp.Services;

/// <summary>
/// Abstraction over a historical-weather data provider. The rest of the app depends on
/// this interface, not on Open-Meteo directly, so the provider could be swapped later
/// (Open-Closed / Dependency Inversion) without touching the aggregator or the API surface.
/// </summary>
public interface IWeatherApiClient
{
    /// <summary>
    /// Fetches min/max temperature and precipitation for a single calendar date.
    /// Never throws for network/API/data problems; failures are represented in the
    /// returned <see cref="WeatherRecord"/>'s Status/ErrorMessage.
    /// </summary>
    Task<WeatherRecord> GetHistoricalWeatherAsync(string rawInput, DateOnly date, CancellationToken cancellationToken = default);
}
