using ChuliTirth.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChuliTirth.ViewComponents;

public class FooterViewComponent : ViewComponent
{
    private readonly ISiteSettingsService _settingsService;
    public FooterViewComponent(ISiteSettingsService settingsService) => _settingsService = settingsService;

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var settings = await _settingsService.GetAllAsync();
        return View(settings);
    }
}
