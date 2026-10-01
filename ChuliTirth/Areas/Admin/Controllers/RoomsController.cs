using ChuliTirth.Interfaces;
using ChuliTirth.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChuliTirth.Areas.Admin.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class RoomsController : AdminControllerBase
{
    private readonly IRoomService _roomService;
    public RoomsController(IRoomService roomService) => _roomService = roomService;

    public async Task<IActionResult> Index() => View(await _roomService.GetAllRoomsAsync());

    public async Task<IActionResult> Create()
    {
        ViewBag.RoomTypes = await _roomService.GetAllRoomTypesAsync();
        return View(new Room { Status = RoomStatus.Available, IsActive = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Room model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.RoomTypes = await _roomService.GetAllRoomTypesAsync();
            return View(model);
        }
        await _roomService.CreateRoomAsync(model);
        TempData["Success"] = "Room created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(int id, RoomStatus status)
    {
        await _roomService.SetRoomStatusAsync(id, status);
        TempData["Success"] = "Room status updated.";
        return RedirectToAction(nameof(Index));
    }
}
