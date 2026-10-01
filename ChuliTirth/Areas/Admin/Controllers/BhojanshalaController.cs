using ChuliTirth.Data;
using ChuliTirth.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChuliTirth.Areas.Admin.Controllers;

public class BhojanshalaController : AdminControllerBase
{
    private readonly ApplicationDbContext _db;
    public BhojanshalaController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index() =>
        View(await _db.BhojanshalaTimings.OrderBy(t => t.DisplayOrder).ToListAsync());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BhojanshalaTiming model)
    {
        if (!ModelState.IsValid) return RedirectToAction(nameof(Index));
        _db.BhojanshalaTimings.Update(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Timing updated.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Create() => View(new BhojanshalaTiming { IsActive = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BhojanshalaTiming model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.BhojanshalaTimings.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Timing added.";
        return RedirectToAction(nameof(Index));
    }
}
