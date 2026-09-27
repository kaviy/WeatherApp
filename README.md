# Dallas Historical Weather — Coding Exercise

A .NET 8 solution that reads dates from `dates.txt`, fetches historical daily weather for
Dallas, TX from the [Open-Meteo Historical Weather API](https://open-meteo.com/en/docs/historical-weather-api),
caches results as JSON, exposes them via a REST endpoint, and displays them in a Razor Pages UI.

## Project layout

```
WeatherApp.sln
WeatherApp/
  Program.cs                     # composition root / DI wiring
  appsettings.json                # location, base URL, timeouts, storage paths (no secrets)
  dates.txt                       # required input file
  Models/                         # POCOs and DTOs
  Services/                       # one interface + implementation per responsibility
    IDatesFileReader / DatesFileReader
    IDateParserService / DateParserService
    IWeatherApiClient / OpenMeteoWeatherApiClient
    IWeatherStorageService / JsonFileWeatherStorageService
    IWeatherAggregatorService / WeatherAggregatorService
  Controllers/
    WeatherController.cs          # GET /api/weather
  Pages/
    Index.cshtml(.cs)             # UI shell; data loads client-side from the API
  wwwroot/
    js/weather.js                 # fetch, loading/error states, sort, filter, row detail
    css/site.css
  weather-data/                   # created at runtime; one JSON file per date (git-ignored)
README.md
AI_NOTES.md
```

## Architecture notes (SOLID)

- **Single Responsibility** — reading the file, parsing dates, calling the API, and caching
  results are each their own class (`DatesFileReader`, `DateParserService`,
  `OpenMeteoWeatherApiClient`, `JsonFileWeatherStorageService`). `WeatherAggregatorService` is
  the only class that coordinates them.
- **Open/Closed** — `IWeatherApiClient` and `IWeatherStorageService` are abstractions; a
  different weather provider or storage backend (e.g. a database) could be added without
  changing the controller, the Razor page, or the aggregator.
- **Liskov Substitution** — implementations only fulfil their interface's contract (e.g. the
  storage service never throws for a missing file — it returns `null`, exactly as the interface
  promises).
- **Interface Segregation** — each service interface exposes only the members its consumers
  need (e.g. `IDateParserService` doesn't know about the filesystem, `IDatesFileReader` doesn't
  know about date formats).
- **Dependency Inversion** — the controller and the aggregator depend only on interfaces,
  registered in `Program.cs`; concrete types are injected by the built-in DI container.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Internet access (Open-Meteo requires no API key, but the app does need to reach
  `archive-api.open-meteo.com`)

## Running the backend + UI

Both the REST API and the Razor Pages UI are hosted by the same ASP.NET Core project, so there
is only one process to run:

```bash
cd WeatherApp
dotnet restore
dotnet run
```

The console output will show the listening URL (typically `https://localhost:5001` or
`http://localhost:5000`). Open it in a browser to see the UI, which calls the API itself.

To call the API directly:

```bash
curl http://localhost:5000/api/weather
```

## Running tests

No automated tests are included given the ~2–3 hour scope of the exercise; see
"What I'd improve" below. The service interfaces (`IDateParserService`, `IWeatherApiClient`,
`IWeatherStorageService`) are designed to be easy to unit test with mocks/fakes if added later.

## Assumptions

- **Location is fixed** to Dallas, TX (`32.78, -96.80`), per the example in the exercise, and is
  configurable via `appsettings.json` (`WeatherApi:Latitude/Longitude`) rather than hardcoded in
  source.
- **Date formats** in `dates.txt` are limited to the three shown in the exercise
  (`MM/dd/yyyy`, `MMMM d, yyyy`, `MMM-dd-yyyy`). Any line that doesn't match one of these is
  reported as an invalid entry rather than crashing the app. `April 31, 2022` is rejected because
  April has 30 days — .NET's `DateTime.TryParseExact` enforces this automatically.
- **Caching key** is the normalized date. If `weather-data/{date}.json` already exists, the
  app serves it without re-calling Open-Meteo, satisfying the "avoid unnecessary repeat calls"
  requirement. Both successful and failed API calls are cached; a future improvement would add a
  short retry/TTL for cached failures instead of caching them indefinitely.
- **One row per line in `dates.txt`**, including invalid ones — invalid dates are returned by
  the API with `status: "Invalid"` so the UI can show them rather than silently dropping them.
- **UI framework**: Razor Pages was chosen (one of the four allowed options) with a single
  vanilla-JS file calling the JSON endpoint, since the exercise says visual design can be
  minimal and clarity matters more than styling/framework choice.

## What I'd improve for production

- Add unit tests for `DateParserService` (format edge cases) and the aggregator (cache hit/miss,
  API failure paths), using fakes for `IWeatherApiClient`/`IWeatherStorageService`.
- Fetch dates concurrently (bounded by a `SemaphoreSlim`) instead of sequentially, and add retry
  with backoff (e.g. Polly) for transient Open-Meteo failures.
- Replace the flat-file cache with a real data store once concurrent writers/multiple instances
  are a concern (the current file-per-date approach isn't safe for concurrent writes to the same
  date from multiple instances).
- Make the location configurable from the UI instead of fixed to Dallas.
- Add structured logging/telemetry around external API latency and error rates.
