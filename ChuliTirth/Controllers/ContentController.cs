using ChuliTirth.Data;
using ChuliTirth.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChuliTirth.Controllers;

public class FacilitiesController : Controller
{
    private readonly IFacilityService _facilityService;
    public FacilitiesController(IFacilityService facilityService) => _facilityService = facilityService;

    public async Task<IActionResult> Index() => View(await _facilityService.GetActiveFacilitiesAsync());
}

public class BhojanshalaController : Controller
{
    private readonly ApplicationDbContext _db;
    public BhojanshalaController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index() =>
        View(await _db.BhojanshalaTimings.Where(t => t.IsActive).OrderBy(t => t.DisplayOrder).AsNoTracking().ToListAsync());
}

public class JainTithiController : Controller
{
    private readonly IJainCalendarService _tithiService;
    public JainTithiController(IJainCalendarService tithiService) => _tithiService = tithiService;

    public async Task<IActionResult> Index()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(5.5));
        return View(await _tithiService.GetUpcomingAsync(today.AddDays(-3), 30));
    }
}

public class GalleryController : Controller
{
    private readonly IGalleryService _galleryService;
    public GalleryController(IGalleryService galleryService) => _galleryService = galleryService;

    public async Task<IActionResult> Index() => View(await _galleryService.GetCategoriesWithImagesAsync());
}

public class AboutController : Controller
{
    private readonly ISiteSettingsService _settingsService;
    public AboutController(ISiteSettingsService settingsService) => _settingsService = settingsService;

    public async Task<IActionResult> Index() => View(await _settingsService.GetAllAsync());
}

public class BookingRulesController : Controller
{
    private readonly ApplicationDbContext _db;
    public BookingRulesController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index() =>
        View(await _db.BookingRules.Where(r => r.IsActive).OrderBy(r => r.DisplayOrder).AsNoTracking().ToListAsync());
}

public class HowToReachController : Controller
{
    private readonly ISiteSettingsService _settingsService;
    public HowToReachController(ISiteSettingsService settingsService) => _settingsService = settingsService;

    public async Task<IActionResult> Index() => View(await _settingsService.GetAllAsync());
}
