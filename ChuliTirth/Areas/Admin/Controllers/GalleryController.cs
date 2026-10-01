using ChuliTirth.Data;
using ChuliTirth.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChuliTirth.Areas.Admin.Controllers;

public class GalleryController : AdminControllerBase
{
    private readonly ApplicationDbContext _db;
    public GalleryController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index() =>
        View(await _db.GalleryCategories.Include(c => c.Images).OrderBy(c => c.DisplayOrder).ToListAsync());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddImage(int categoryId, string imageUrl, string? caption)
    {
        _db.GalleryImages.Add(new GalleryImage { GalleryCategoryId = categoryId, ImageUrl = imageUrl, Caption = caption, IsActive = true });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Image added.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(int id)
    {
        var image = await _db.GalleryImages.FindAsync(id);
        if (image is not null)
        {
            _db.GalleryImages.Remove(image);
            await _db.SaveChangesAsync();
        }
        TempData["Success"] = "Image removed.";
        return RedirectToAction(nameof(Index));
    }
}
