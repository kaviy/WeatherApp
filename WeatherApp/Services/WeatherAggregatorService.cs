using WeatherApp.Models;

namespace WeatherApp.Services;

public class WeatherAggregatorService : IWeatherAggregatorService
{
    private readonly IDatesFileReader _datesFileReader;
    private readonly IDateParserService _dateParser;
    private readonly IWeatherStorageService _storage;
    private readonly IWeatherApiClient _weatherApiClient;
    private readonly ILogger<WeatherAggregatorService> _logger;

    public WeatherAggregatorService(
        IDatesFileReader datesFileReader,
        IDateParserService dateParser,
        IWeatherStorageService storage,
        IWeatherApiClient weatherApiClient,
        ILogger<WeatherAggregatorService> logger)
    {
        _datesFileReader = datesFileReader;
        _dateParser = dateParser;
        _storage = storage;
        _weatherApiClient = weatherApiClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<WeatherRecord>> GetWeatherAsync(CancellationToken cancellationToken = default)
    {
        var rawLines = await _datesFileReader.ReadLinesAsync(cancellationToken);
        var parseResults = _dateParser.ParseDates(rawLines).ToList();

        var records = new List<WeatherRecord>(parseResults.Count);

        foreach (var parseResult in parseResults)
        {
            if (!parseResult.IsValid || parseResult.ParsedDate is null)
            {
                records.Add(WeatherRecord.Invalid(
                    parseResult.RawInput,
                    parseResult.ErrorMessage ?? "Invalid date."));
                continue;
            }

            var date = parseResult.ParsedDate.Value;
            records.Add(await GetOrFetchAsync(parseResult.RawInput, date, cancellationToken));
        }

        return records;
    }

    private async Task<WeatherRecord> GetOrFetchAsync(
        string rawInput, DateOnly date, CancellationToken cancellationToken)
    {
        if (_storage.Exists(date))
        {
            var cached = await _storage.LoadAsync(date, cancellationToken);
            if (cached is not null)
            {
                return cached;
            }

            _logger.LogWarning("Cache file existed but could not be read for {Date}; re-fetching.", date);
        }

        var fetched = await _weatherApiClient.GetHistoricalWeatherAsync(rawInput, date, cancellationToken);

        // Cache successes and failures alike, so a persistently-bad date (e.g. API outage)
        // doesn't get silently retried forever on every page load. A future improvement
        // could add a short TTL/backoff before retrying cached errors.
        await _storage.SaveAsync(fetched, date, cancellationToken);

        return fetched;
    }
}
