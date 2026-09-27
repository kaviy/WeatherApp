using Microsoft.Extensions.Options;
using WeatherApp.Models;

namespace WeatherApp.Services;

public class DatesFileReader : IDatesFileReader
{
    private readonly string _filePath;
    private readonly ILogger<DatesFileReader> _logger;

    public DatesFileReader(
        IOptions<WeatherStorageOptions> options,
        IWebHostEnvironment environment,
        ILogger<DatesFileReader> logger)
    {
        _logger = logger;

        _filePath = Path.IsPathRooted(options.Value.DatesFilePath)
            ? options.Value.DatesFilePath
            : Path.Combine(environment.ContentRootPath, options.Value.DatesFilePath);
    }

    public async Task<IReadOnlyList<string>> ReadLinesAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            _logger.LogWarning("Dates file not found at {Path}.", _filePath);
            return Array.Empty<string>();
        }

        return await File.ReadAllLinesAsync(_filePath, cancellationToken);
    }
}
