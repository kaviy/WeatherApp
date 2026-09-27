namespace WeatherApp.Models;

/// <summary>
/// Status of a single date's weather lookup. Kept as a string in the JSON payload
/// for readability in stored files and in the browser, backed by this enum in code.
/// </summary>
public enum WeatherRecordStatus
{
    Ok,
    Invalid,
    Error
}

/// <summary>
/// A single row of the result set: one entry per line in dates.txt.
/// This is the shape persisted to weather-data/*.json and returned by GET /api/weather.
/// </summary>
public class WeatherRecord
{
    /// <summary>Original text as it appeared in dates.txt.</summary>
    public string RawInput { get; set; } = string.Empty;

    /// <summary>Normalized ISO date (yyyy-MM-dd), null if the input could not be parsed.</summary>
    public string? Date { get; set; }

    public double? MinTemperature { get; set; }
    public double? MaxTemperature { get; set; }
    public double? PrecipitationSum { get; set; }

    public WeatherRecordStatus Status { get; set; } = WeatherRecordStatus.Ok;
    public string? ErrorMessage { get; set; }

    public static WeatherRecord Invalid(string rawInput, string errorMessage) => new()
    {
        RawInput = rawInput,
        Date = null,
        Status = WeatherRecordStatus.Invalid,
        ErrorMessage = errorMessage
    };

    public static WeatherRecord Failed(string rawInput, string date, string errorMessage) => new()
    {
        RawInput = rawInput,
        Date = date,
        Status = WeatherRecordStatus.Error,
        ErrorMessage = errorMessage
    };

    public static WeatherRecord Ok(string rawInput, string date, double? min, double? max, double? precipitation) => new()
    {
        RawInput = rawInput,
        Date = date,
        MinTemperature = min,
        MaxTemperature = max,
        PrecipitationSum = precipitation,
        Status = WeatherRecordStatus.Ok
    };
}
