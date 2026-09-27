using WeatherApp.Models;
using WeatherApp.Services;

var builder = WebApplication.CreateBuilder(args);

// ---- Configuration (strongly typed, no hardcoded secrets/values in code) ----
builder.Services.Configure<WeatherApiOptions>(
    builder.Configuration.GetSection(WeatherApiOptions.SectionName));
builder.Services.Configure<WeatherStorageOptions>(
    builder.Configuration.GetSection(WeatherStorageOptions.SectionName));

// ---- HTTP client for the external weather provider ----
builder.Services.AddHttpClient<IWeatherApiClient, OpenMeteoWeatherApiClient>((sp, client) =>
{
    var options = builder.Configuration.GetSection(WeatherApiOptions.SectionName).Get<WeatherApiOptions>()
                  ?? new WeatherApiOptions();

    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds > 0 ? options.TimeoutSeconds : 30);
});

// ---- Application services (each interface has one job; see Services/ for details) ----
builder.Services.AddSingleton<IDateParserService, DateParserService>();
builder.Services.AddSingleton<IDatesFileReader, DatesFileReader>();
builder.Services.AddSingleton<IWeatherStorageService, JsonFileWeatherStorageService>();
builder.Services.AddScoped<IWeatherAggregatorService, WeatherAggregatorService>();

// ---- Web layers: REST API + Razor Pages UI ----
// WeatherRecordStatus is serialized as a readable string ("Ok"/"Invalid"/"Error")
// instead of a numeric enum, matching the same convention used by the JSON cache files.
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();
