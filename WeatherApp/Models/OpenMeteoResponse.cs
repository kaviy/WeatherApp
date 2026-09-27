using System.Text.Json.Serialization;

namespace WeatherApp.Models;

/// <summary>
/// Deserialization target for the Open-Meteo /v1/archive response.
/// Only the fields this app needs are mapped; unknown fields are ignored by default.
/// </summary>
public class OpenMeteoResponse
{
    [JsonPropertyName("daily")]
    public OpenMeteoDaily? Daily { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; } // present on some Open-Meteo error payloads
}

public class OpenMeteoDaily
{
    [JsonPropertyName("time")]
    public List<string>? Time { get; set; }

    [JsonPropertyName("temperature_2m_max")]
    public List<double?>? TemperatureMax { get; set; }

    [JsonPropertyName("temperature_2m_min")]
    public List<double?>? TemperatureMin { get; set; }

    [JsonPropertyName("precipitation_sum")]
    public List<double?>? PrecipitationSum { get; set; }
}
