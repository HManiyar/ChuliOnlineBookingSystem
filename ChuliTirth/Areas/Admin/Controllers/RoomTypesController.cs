using ChuliTirth.Interfaces;
using ChuliTirth.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChuliTirth.Areas.Admin.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class RoomTypesController : AdminControllerBase
{
    private readonly IRoomService _roomService;
    public RoomTypesController(IRoomService roomService) => _roomService = roomService;

    public async Task<IActionResult> Index() => View(await _roomService.GetAllRoomTypesAsync());

    public IActionResult Create() => View(new RoomType { IsActive = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoomType model)
    {
        if (!ModelState.IsValid) return View(model);
        await _roomService.CreateRoomTypeAsync(model);
        TempData["Success"] = "Room type created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var rt = await _roomService.GetRoomTypeByIdAsync(id);
        if (rt is null) return NotFound();
        return View(rt);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, RoomType model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        await _roomService.UpdateRoomTypeAsync(model);
        TempData["Success"] = "Room type updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _roomService.DeactivateRoomTypeAsync(id);
        TempData["Success"] = "Room type deactivated.";
        return RedirectToAction(nameof(Index));
    }
}
