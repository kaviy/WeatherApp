# AI / Vibe Coding Usage Notes

## 1. Which AI tool(s) I used

This solution was built with **Claude** (Anthropic), working directly from the exercise PDF in a
single conversation: I gave Claude the exercise document and asked it to generate the full
solution (Razor Pages + REST API, .NET 8, SOLID principles), then reviewed and adjusted the
generated code and file layout.

## 2. Prompts that helped the most

1. *"Generate code based on this document using Razor Pages and REST APIs, .NET 8, SOLID
   principles"* — the initial prompt that produced the overall project skeleton (Models /
   Services / Controllers / Pages split) rather than one monolithic Program.cs.
2. *"Make sure invalid dates like April 31 are rejected, not silently rolled over, and handled
   without crashing"* — this pushed the date-parsing approach toward `DateTime.TryParseExact`
   with explicit format strings and `DateTimeStyles.NoCurrentDateDefault`, instead of a looser
   parse that could "fix" bad input by accident.
3. *"Add a UI interaction beyond just displaying the table — sorting and a way to inspect a
   single date"* — led to the click-to-sort column headers and the click-a-row detail panel in
   `weather.js`, instead of a static read-only table.

## 3. Where the AI's suggestion was wrong or not ideal, and how I corrected it

**Date parsing.** An early version used `DateTime.TryParse(input, out var result)` with no
explicit formats, relying on .NET's general-purpose parser to "figure out" formats like
`Jul-13-2020`. Two problems surfaced on review:

- General `TryParse` is culture- and format-ambiguous (e.g. it can misinterpret day/month order
  depending on the runtime's culture settings), which is risky for a requirement that explicitly
  tests format handling.
- More importantly, loosely-configured date parsing can *normalize* an invalid date instead of
  rejecting it (e.g. treating `"April 31"` as `"May 1"`), which directly violates the
  requirement to **treat `April 31, 2022` as invalid**, not silently correct it.

The fix was to switch to `DateTime.TryParseExact` against the three exact formats used in
`dates.txt` (`MM/dd/yyyy`, `MMMM d, yyyy`, `MMM-dd-yyyy`) with `DateTimeStyles.NoCurrentDateDefault`.
`TryParseExact` validates the day against the actual number of days in that month/year, so
`April 31` correctly fails to parse instead of being coerced into a real date. I verified this
reasoning against the exercise's explicit example before accepting it.

## 4. Parts I chose to write/adjust myself rather than rely on AI as-is

- **Caching semantics**: I decided that both successful *and* failed API responses should be
  written to `weather-data/*.json` (with `status: "Error"`), rather than only caching successes.
  This was a deliberate trade-off — it satisfies the "avoid unnecessary repeat calls" requirement
  even for a date that failed, at the cost of needing a future TTL/backoff so a transient failure
  isn't cached forever. I noted this explicitly as a known limitation in the README rather than
  letting the AI's first pass (cache-success-only) stand unexamined.
- **Error handling boundaries**: I kept per-date failures (network error, empty data, bad date)
  inside `WeatherRecord.Status`/`ErrorMessage` so one bad date never fails the whole request, and
  reserved the controller's try/catch for truly unexpected failures (e.g. misconfiguration) —
  this separation of "expected, per-item failure" vs. "unexpected failure" was a design decision
  I made explicit rather than leaving implicit in generated code.
- **Configuration**: moved latitude/longitude/base URL/storage paths into `appsettings.json`
  behind `WeatherApiOptions`/`WeatherStorageOptions` POCOs instead of leaving them as inline
  constants, per the "treat config cleanly" requirement.
 -- **Add CSS Styles**: Added site.css (WeatherApp/wwwroot/css/site.css) to maintain all CSS classes in one place, referenced from _Layout.cshtml, instead of inline styles scattered across the pages.
