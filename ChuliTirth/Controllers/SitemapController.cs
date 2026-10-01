using ChuliTirth.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace ChuliTirth.Controllers;

public class SitemapController : Controller
{
    private readonly IRoomService _roomService;
    public SitemapController(IRoomService roomService) => _roomService = roomService;

    [Route("sitemap.xml")]
    public async Task<IActionResult> Index()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var staticPaths = new[]
        {
            "/", "/Rooms", "/Facilities", "/Bhojanshala", "/JainTithi", "/Gallery",
            "/About", "/BookingRules", "/HowToReach", "/Contact"
        };

        var sb = new StringBuilder();
        sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.Append("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

        foreach (var path in staticPaths)
        {
            sb.Append($"<url><loc>{baseUrl}{path}</loc></url>");
        }

        var roomTypes = await _roomService.GetActiveRoomTypesAsync();
        foreach (var rt in roomTypes)
        {
            sb.Append($"<url><loc>{baseUrl}/Rooms/Details/{rt.Id}</loc></url>");
        }

        sb.Append("</urlset>");
        return Content(sb.ToString(), "application/xml");
    }
}
