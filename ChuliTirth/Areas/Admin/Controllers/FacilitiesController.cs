using ChuliTirth.Interfaces;
using ChuliTirth.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ChuliTirth.Areas.Admin.Controllers;

public class FacilitiesController : AdminControllerBase
{
    private readonly IFacilityService _facilityService;
    public FacilitiesController(IFacilityService facilityService) => _facilityService = facilityService;

    public async Task<IActionResult> Index() => View(await _facilityService.GetAllAsync());

    public IActionResult Create() => View(new Facility { IsActive = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Facility model)
    {
        if (!ModelState.IsValid) return View(model);
        await _facilityService.CreateAsync(model);
        TempData["Success"] = "Facility added.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var facility = (await _facilityService.GetAllAsync()).FirstOrDefault(f => f.Id == id);
        if (facility is null) return NotFound();
        return View(facility);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Facility model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        await _facilityService.UpdateAsync(model);
        TempData["Success"] = "Facility updated.";
        return RedirectToAction(nameof(Index));
    }
}
