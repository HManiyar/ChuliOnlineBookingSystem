using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChuliTirth.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Manager,Staff")]
public abstract class AdminControllerBase : Controller
{
}
