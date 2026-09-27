using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WeatherApp.Pages;

/// <summary>
/// Deliberately thin: the page's only job is to render the shell. All weather data
/// is loaded client-side from GET /api/weather, which keeps the UI and the REST API
/// decoupled (the same endpoint could serve any other client).
/// </summary>
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}
