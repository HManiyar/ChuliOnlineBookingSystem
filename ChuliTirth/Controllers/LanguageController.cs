using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace ChuliTirth.Controllers;

public class LanguageController : Controller
{
    [HttpGet]
    public IActionResult SetCulture(string culture, string returnUrl = "/")
    {
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true }
        );

        if (!Url.IsLocalUrl(returnUrl)) returnUrl = "/";
        return LocalRedirect(returnUrl);
    }
}
