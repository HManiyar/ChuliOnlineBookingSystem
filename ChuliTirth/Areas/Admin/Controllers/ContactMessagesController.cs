using ChuliTirth.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChuliTirth.Areas.Admin.Controllers;

public class ContactMessagesController : AdminControllerBase
{
    private readonly ApplicationDbContext _db;
    public ContactMessagesController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index() =>
        View(await _db.ContactMessages.OrderByDescending(m => m.CreatedAtUtc).ToListAsync());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id)
    {
        var msg = await _db.ContactMessages.FindAsync(id);
        if (msg is not null)
        {
            msg.IsRead = true;
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
