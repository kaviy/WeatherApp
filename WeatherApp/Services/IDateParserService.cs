using WeatherApp.Models;

namespace WeatherApp.Services;

/// <summary>
/// Parses free-form date strings (as found in dates.txt) into normalized dates.
/// Kept separate from I/O (reading the file) so it is independently unit-testable.
/// </summary>
public interface IDateParserService
{
    /// <summary>
    /// Attempts to parse each raw line into a date. Never throws for malformed input;
    /// invalid lines come back as a failed <see cref="DateParseResult"/> instead.
    /// </summary>
    IEnumerable<DateParseResult> ParseDates(IEnumerable<string> rawLines);
}
