using System.Globalization;
using WeatherApp.Models;

namespace WeatherApp.Services;

/// <summary>
/// Parses the date formats used in dates.txt:
///   02/27/2021   -> MM/dd/yyyy
///   June 2, 2022 -> MMMM d, yyyy
///   Jul-13-2020  -> MMM-dd-yyyy
/// Calendar validity (e.g. "April 31") is enforced by DateTime.TryParseExact itself,
/// which rejects impossible day/month combinations rather than silently rolling over.
/// </summary>
public class DateParserService : IDateParserService
{
    private static readonly string[] SupportedFormats =
    {
        "MM/dd/yyyy",
        "MMMM d, yyyy",
        "MMM-dd-yyyy"
    };

    public IEnumerable<DateParseResult> ParseDates(IEnumerable<string> rawLines)
    {
        foreach (var line in rawLines)
        {
            var trimmed = line?.Trim();

            if (string.IsNullOrWhiteSpace(trimmed))
            {
                continue; // skip blank lines silently, not an error
            }

            if (TryParse(trimmed, out var parsedDate))
            {
                yield return DateParseResult.Success(trimmed, parsedDate);
            }
            else
            {
                yield return DateParseResult.Failure(
                    trimmed,
                    $"'{trimmed}' could not be parsed as a valid date in any supported format.");
            }
        }
    }

    private static bool TryParse(string input, out DateOnly result)
    {
        if (DateTime.TryParseExact(
                input,
                SupportedFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.NoCurrentDateDefault,
                out var parsed))
        {
            result = DateOnly.FromDateTime(parsed);
            return true;
        }

        result = default;
        return false;
    }
}
