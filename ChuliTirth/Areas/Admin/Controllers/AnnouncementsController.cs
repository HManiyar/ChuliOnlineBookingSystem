using ChuliTirth.Interfaces;
using ChuliTirth.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ChuliTirth.Areas.Admin.Controllers;

public class AnnouncementsController : AdminControllerBase
{
    private readonly IAnnouncementService _announcementService;
    public AnnouncementsController(IAnnouncementService announcementService) => _announcementService = announcementService;

    public async Task<IActionResult> Index() => View(await _announcementService.GetAllAsync());

    public IActionResult Create() => View(new Announcement { IsActive = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Announcement model)
    {
        if (!ModelState.IsValid) return View(model);
        await _announcementService.CreateAsync(model);
        TempData["Success"] = "Announcement created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var all = await _announcementService.GetAllAsync();
        var item = all.FirstOrDefault(a => a.Id == id);
        if (item is null) return NotFound();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Announcement model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        await _announcementService.UpdateAsync(model);
        TempData["Success"] = "Announcement updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _announcementService.DeleteAsync(id);
        TempData["Success"] = "Announcement deleted.";
        return RedirectToAction(nameof(Index));
    }
}
