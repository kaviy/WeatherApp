namespace WeatherApp.Models;

/// <summary>
/// Outcome of attempting to parse a single raw date string from dates.txt.
/// Invalid input is represented explicitly rather than throwing, so callers
/// can handle it gracefully (requirement: "Handle invalid dates gracefully").
/// </summary>
public class DateParseResult
{
    public required string RawInput { get; init; }
    public bool IsValid { get; init; }
    public DateOnly? ParsedDate { get; init; }
    public string? ErrorMessage { get; init; }

    public static DateParseResult Success(string rawInput, DateOnly parsedDate) => new()
    {
        RawInput = rawInput,
        IsValid = true,
        ParsedDate = parsedDate
    };

    public static DateParseResult Failure(string rawInput, string errorMessage) => new()
    {
        RawInput = rawInput,
        IsValid = false,
        ErrorMessage = errorMessage
    };
}
