namespace WeatherApp.Services;

/// <summary>
/// Reads the raw lines of dates.txt. Kept separate from IDateParserService so
/// parsing logic can be unit-tested without touching the filesystem.
/// </summary>
public interface IDatesFileReader
{
    Task<IReadOnlyList<string>> ReadLinesAsync(CancellationToken cancellationToken = default);
}
