using WeatherApp.Models;

namespace WeatherApp.Services;

/// <summary>
/// Persists and retrieves per-date weather results as local JSON files under weather-data/.
/// Isolated behind an interface so the aggregator does not care whether storage is
/// the filesystem, a database, or a cache (Dependency Inversion).
/// </summary>
public interface IWeatherStorageService
{
    /// <summary>True if a result for this date is already stored (used to skip repeat API calls).</summary>
    bool Exists(DateOnly date);

    Task<WeatherRecord?> LoadAsync(DateOnly date, CancellationToken cancellationToken = default);

    Task SaveAsync(WeatherRecord record, DateOnly date, CancellationToken cancellationToken = default);
}
