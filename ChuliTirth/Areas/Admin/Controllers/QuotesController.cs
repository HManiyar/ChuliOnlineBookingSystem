using ChuliTirth.Interfaces;
using ChuliTirth.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ChuliTirth.Areas.Admin.Controllers;

public class QuotesController : AdminControllerBase
{
    private readonly IJainQuoteService _quoteService;
    public QuotesController(IJainQuoteService quoteService) => _quoteService = quoteService;

    public async Task<IActionResult> Index() => View(await _quoteService.GetAllAsync());

    public IActionResult Create() => View(new JainQuote { IsActive = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(JainQuote model)
    {
        if (!ModelState.IsValid) return View(model);
        await _quoteService.CreateAsync(model);
        TempData["Success"] = "Quote added.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var quote = (await _quoteService.GetAllAsync()).FirstOrDefault(q => q.Id == id);
        if (quote is null) return NotFound();
        return View(quote);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, JainQuote model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        await _quoteService.UpdateAsync(model);
        TempData["Success"] = "Quote updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _quoteService.DeleteAsync(id);
        TempData["Success"] = "Quote deleted.";
        return RedirectToAction(nameof(Index));
    }
}
