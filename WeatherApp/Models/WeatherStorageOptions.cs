namespace WeatherApp.Models;

/// <summary>
/// Strongly typed options bound from the "WeatherStorage" configuration section.
/// </summary>
public class WeatherStorageOptions
{
    public const string SectionName = "WeatherStorage";

    public string DirectoryPath { get; set; } = "weather-data";
    public string DatesFilePath { get; set; } = "dates.txt";
}
