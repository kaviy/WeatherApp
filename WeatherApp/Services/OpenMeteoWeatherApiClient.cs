using System.Globalization;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using WeatherApp.Models;

namespace WeatherApp.Services;

/// <summary>
/// Calls the Open-Meteo Historical Weather API (/v1/archive) for a fixed location
/// (configured via WeatherApiOptions) and maps the response into a WeatherRecord.
/// </summary>
public class OpenMeteoWeatherApiClient : IWeatherApiClient
{
    private readonly HttpClient _httpClient;
    private readonly WeatherApiOptions _options;
    private readonly ILogger<OpenMeteoWeatherApiClient> _logger;

    public OpenMeteoWeatherApiClient(
        HttpClient httpClient,
        IOptions<WeatherApiOptions> options,
        ILogger<OpenMeteoWeatherApiClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<WeatherRecord> GetHistoricalWeatherAsync(
        string rawInput, DateOnly date, CancellationToken cancellationToken = default)
    {
        var isoDate = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        var requestUri =
            "v1/archive" +
            $"?latitude={_options.Latitude.ToString(CultureInfo.InvariantCulture)}" +
            $"&longitude={_options.Longitude.ToString(CultureInfo.InvariantCulture)}" +
            $"&start_date={isoDate}&end_date={isoDate}" +
            "&daily=temperature_2m_max,temperature_2m_min,precipitation_sum" +
            "&timezone=auto";

        try
        {
            using var response = await _httpClient.GetAsync(requestUri, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Open-Meteo returned {StatusCode} for {Date}", response.StatusCode, isoDate);

                return WeatherRecord.Failed(
                    rawInput, isoDate, $"Weather API returned HTTP {(int)response.StatusCode}.");
            }

            var payload = await response.Content.ReadFromJsonAsync<OpenMeteoResponse>(
                cancellationToken: cancellationToken);

            var daily = payload?.Daily;

            if (daily?.Time is null || daily.Time.Count == 0)
            {
                return WeatherRecord.Failed(
                    rawInput, isoDate, payload?.Reason ?? "No weather data was returned for this date.");
            }

            return WeatherRecord.Ok(
                rawInput,
                isoDate,
                min: daily.TemperatureMin?.FirstOrDefault(),
                max: daily.TemperatureMax?.FirstOrDefault(),
                precipitation: daily.PrecipitationSum?.FirstOrDefault());
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "Open-Meteo request timed out for {Date}", isoDate);
            return WeatherRecord.Failed(rawInput, isoDate, "The request to the weather API timed out.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error calling Open-Meteo for {Date}", isoDate);
            return WeatherRecord.Failed(rawInput, isoDate, $"Network error contacting weather API: {ex.Message}");
        }
        catch (System.Text.Json.JsonException ex)
        {
            _logger.LogError(ex, "Malformed JSON from Open-Meteo for {Date}", isoDate);
            return WeatherRecord.Failed(rawInput, isoDate, "Weather API returned an unexpected response format.");
        }
    }
}
