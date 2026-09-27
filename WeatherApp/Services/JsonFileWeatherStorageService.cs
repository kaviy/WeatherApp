using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using WeatherApp.Models;

namespace WeatherApp.Services;

/// <summary>
/// Stores each date's WeatherRecord as weather-data/{yyyy-MM-dd}.json under the
/// content root. A simple file-existence check lets the aggregator avoid
/// re-calling the external API for dates already fetched.
/// </summary>
public class JsonFileWeatherStorageService : IWeatherStorageService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _directoryPath;
    private readonly ILogger<JsonFileWeatherStorageService> _logger;

    public JsonFileWeatherStorageService(
        IOptions<WeatherStorageOptions> options,
        IWebHostEnvironment environment,
        ILogger<JsonFileWeatherStorageService> logger)
    {
        _logger = logger;

        _directoryPath = Path.IsPathRooted(options.Value.DirectoryPath)
            ? options.Value.DirectoryPath
            : Path.Combine(environment.ContentRootPath, options.Value.DirectoryPath);

        Directory.CreateDirectory(_directoryPath);
    }

    public bool Exists(DateOnly date) => File.Exists(GetFilePath(date));

    public async Task<WeatherRecord?> LoadAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var path = GetFilePath(date);

        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            await using var stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<WeatherRecord>(stream, cancellationToken: cancellationToken);
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            // A corrupted or partially-written cache file should not crash the request;
            // treat it as a cache miss so the caller re-fetches from the API.
            _logger.LogWarning(ex, "Could not read cached weather file {Path}; treating as missing.", path);
            return null;
        }
    }

    public async Task SaveAsync(WeatherRecord record, DateOnly date, CancellationToken cancellationToken = default)
    {
        var path = GetFilePath(date);

        try
        {
            await using var stream = File.Create(path);
            await JsonSerializer.SerializeAsync(stream, record, SerializerOptions, cancellationToken);
        }
        catch (IOException ex)
        {
            // Failing to cache is not fatal to the request; log and move on.
            _logger.LogWarning(ex, "Could not write weather cache file {Path}.", path);
        }
    }

    private string GetFilePath(DateOnly date) =>
        Path.Combine(_directoryPath, $"{date:yyyy-MM-dd}.json");
}
