using ChuliTirth.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChuliTirth.ViewComponents;

public class AnnouncementBannerViewComponent : ViewComponent
{
    private readonly IAnnouncementService _announcementService;
    public AnnouncementBannerViewComponent(IAnnouncementService announcementService) => _announcementService = announcementService;

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var announcements = await _announcementService.GetActiveAsync();
        return View(announcements);
    }
}
