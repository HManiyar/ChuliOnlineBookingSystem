using ChuliTirth.Data;
using ChuliTirth.Models.Entities;
using ChuliTirth.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ChuliTirth.Controllers;

public class ContactController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ContactController> _logger;

    public ContactController(ApplicationDbContext db, ILogger<ContactController> logger)
    {
        _db = db;
        _logger = logger;
    }

    public IActionResult Index() => View(new ContactFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ContactFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        _db.ContactMessages.Add(new ContactMessage
        {
            Name = model.Name,
            Email = model.Email,
            Mobile = model.Mobile,
            Subject = model.Subject,
            Message = model.Message
        });
        await _db.SaveChangesAsync();
        _logger.LogInformation("Contact message received from {Email}", model.Email);

        TempData["Success"] = "Thank you. Your message has been received and our team will respond shortly.";
        return RedirectToAction(nameof(Index));
    }
}
