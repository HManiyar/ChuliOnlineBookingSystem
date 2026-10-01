using ChuliTirth.Data;
using ChuliTirth.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChuliTirth.Areas.Admin.Controllers;

public class TithisController : AdminControllerBase
{
    private readonly ApplicationDbContext _db;
    public TithisController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index() =>
        View(await _db.JainTithis.OrderBy(t => t.GregorianDate).ToListAsync());

    public IActionResult Create() => View(new JainTithi { GregorianDate = DateOnly.FromDateTime(DateTime.Today), IsActive = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(JainTithi model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.JainTithis.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Tithi entry added.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var t = await _db.JainTithis.FindAsync(id);
        if (t is null) return NotFound();
        return View(t);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, JainTithi model)
    {
        if (id != model.Id) return NotFound();
        if (!ModelState.IsValid) return View(model);

        var t = await _db.JainTithis.FindAsync(id);
        if (t is null) return NotFound();

        t.GregorianDate = model.GregorianDate;
        t.Paksha = model.Paksha;
        t.PakshaGujarati = model.PakshaGujarati;
        t.PakshaHindi = model.PakshaHindi;
        t.TithiName = model.TithiName;
        t.TithiNameGujarati = model.TithiNameGujarati;
        t.TithiNameHindi = model.TithiNameHindi;
        t.SpecialOccasion = model.SpecialOccasion;
        t.SpecialOccasionGujarati = model.SpecialOccasionGujarati;
        t.SpecialOccasionHindi = model.SpecialOccasionHindi;
        t.IsActive = model.IsActive;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Tithi entry updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var t = await _db.JainTithis.FindAsync(id);
        if (t is not null)
        {
            _db.JainTithis.Remove(t);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
