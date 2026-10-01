using ChuliTirth.Interfaces;
using ChuliTirth.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ChuliTirth.Controllers;

public class HomeController : Controller
{
    private readonly IRoomService _roomService;
    private readonly IFacilityService _facilityService;
    private readonly IAnnouncementService _announcementService;
    private readonly IGalleryService _galleryService;
    private readonly IJainCalendarService _tithiService;
    private readonly ISiteSettingsService _settingsService;

    public HomeController(IRoomService roomService, IFacilityService facilityService, IAnnouncementService announcementService,
        IGalleryService galleryService, IJainCalendarService tithiService, ISiteSettingsService settingsService)
    {
        _roomService = roomService;
        _facilityService = facilityService;
        _announcementService = announcementService;
        _galleryService = galleryService;
        _tithiService = tithiService;
        _settingsService = settingsService;
    }

    public async Task<IActionResult> Index()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(5.5));
        var galleryCategories = await _galleryService.GetCategoriesWithImagesAsync();

        var vm = new HomeViewModel
        {
            RoomTypes = await _roomService.GetActiveRoomTypesAsync(),
            Facilities = await _facilityService.GetActiveFacilitiesAsync(),
            Announcements = await _announcementService.GetActiveAsync(),
            GalleryPreview = galleryCategories.SelectMany(c => c.Images).Take(8).ToList(),
            TodayTithi = await _tithiService.GetTithiForDateAsync(today),
            Settings = await _settingsService.GetAllAsync(),
            Search = new BookingSearchViewModel
            {
                CheckIn = today.AddDays(1),
                CheckOut = today.AddDays(2)
            }
        };
        return View(vm);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => RedirectToAction("HandleException", "Error");
}
