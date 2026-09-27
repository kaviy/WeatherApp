namespace WeatherApp.Models;

/// <summary>
/// Strongly typed options bound from the "WeatherApi" configuration section.
/// Keeps HTTP client configuration out of code, per the "no hardcoded config" requirement.
/// </summary>
public class WeatherApiOptions
{
    public const string SectionName = "WeatherApi";

    public string BaseUrl { get; set; } = "https://archive-api.open-meteo.com/";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
}
