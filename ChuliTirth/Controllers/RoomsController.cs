using ChuliTirth.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChuliTirth.Controllers;

public class RoomsController : Controller
{
    private readonly IRoomService _roomService;
    private readonly IRoomAvailabilityService _availabilityService;

    public RoomsController(IRoomService roomService, IRoomAvailabilityService availabilityService)
    {
        _roomService = roomService;
        _availabilityService = availabilityService;
    }

    public async Task<IActionResult> Index()
    {
        var roomTypes = await _roomService.GetActiveRoomTypesAsync();
        return View(roomTypes);
    }

    public async Task<IActionResult> Details(int id)
    {
        var roomType = await _roomService.GetRoomTypeByIdAsync(id);
        if (roomType is null || !roomType.IsActive) return NotFound();

        var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(5.5));
        ViewBag.Calendar = await _availabilityService.GetCalendarAsync(id, today, today.AddDays(30));
        return View(roomType);
    }
}
