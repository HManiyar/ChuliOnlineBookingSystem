using ChuliTirth.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChuliTirth.Areas.Admin.Controllers;

[Authorize(Roles = "Admin")]
public class SettingsController : AdminControllerBase
{
    private static readonly string[] Keys =
    {
        "DharamshalaName", "Address", "Phone", "Email", "WhatsApp", "GoogleMapsUrl",
        "CheckInTime", "CheckOutTime", "HomepageHeroText", "FooterText"
    };

    private readonly ISiteSettingsService _settingsService;
    public SettingsController(ISiteSettingsService settingsService) => _settingsService = settingsService;

    public async Task<IActionResult> Index()
    {
        var all = await _settingsService.GetAllAsync();
        foreach (var key in Keys)
        {
            all.TryAdd(key, null);
        }
        return View(all);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(Dictionary<string, string?> values)
    {
        foreach (var (key, value) in values)
        {
            await _settingsService.SetAsync(key, value);
        }
        TempData["Success"] = "Settings saved.";
        return RedirectToAction(nameof(Index));
    }
}
