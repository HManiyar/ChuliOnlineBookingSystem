using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ChuliTirth.Controllers;

public class ErrorController : Controller
{
    private readonly ILogger<ErrorController> _logger;
    public ErrorController(ILogger<ErrorController> logger) => _logger = logger;

    [Route("Error/{statusCode:int}")]
    public IActionResult HandleStatusCode(int statusCode)
    {
        Response.StatusCode = statusCode;
        return statusCode switch
        {
            404 => View("NotFound"),
            403 => View("Forbidden"),
            _ => View("GenericError", statusCode)
        };
    }

    [Route("Error/500")]
    public IActionResult HandleException()
    {
        var feature = HttpContext.Features.Get<IExceptionHandlerFeature>();
        if (feature?.Error is not null)
        {
            _logger.LogError(feature.Error, "Unhandled exception on {Path}", feature.Path);
        }
        Response.StatusCode = 500;
        return View("GenericError", 500);
    }
}
