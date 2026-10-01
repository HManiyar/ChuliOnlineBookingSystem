using ChuliTirth.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChuliTirth.ViewComponents;

public class TithiTickerViewComponent : ViewComponent
{
    private readonly IJainCalendarService _tithiService;
    public TithiTickerViewComponent(IJainCalendarService tithiService) => _tithiService = tithiService;

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(5.5));
        var upcoming = await _tithiService.GetUpcomingAsync(today, 7);
        return View(upcoming);
    }
}
