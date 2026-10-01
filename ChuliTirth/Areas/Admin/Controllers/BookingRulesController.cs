using ChuliTirth.Data;
using ChuliTirth.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChuliTirth.Areas.Admin.Controllers;

public class BookingRulesController : AdminControllerBase
{
    private readonly ApplicationDbContext _db;
    public BookingRulesController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index() =>
        View(await _db.BookingRules.OrderBy(r => r.DisplayOrder).ToListAsync());

    public IActionResult Create() => View(new BookingRule { IsActive = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingRule model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.BookingRules.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Rule added.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BookingRule model)
    {
        if (!ModelState.IsValid) return RedirectToAction(nameof(Index));
        _db.BookingRules.Update(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Rule updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var rule = await _db.BookingRules.FindAsync(id);
        if (rule is not null)
        {
            _db.BookingRules.Remove(rule);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
