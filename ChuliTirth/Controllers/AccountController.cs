using System.Security.Claims;
using ChuliTirth.Interfaces;
using ChuliTirth.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChuliTirth.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly IAuthMailer _authMailer;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IAuthService authService, IAuthMailer authMailer, ILogger<AccountController> logger)
    {
        _authService = authService;
        _authMailer = authMailer;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Register(string? returnUrl = null) => View(new RegisterViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _authService.RegisterAsync(model.FullName, model.Email, model.Mobile, model.Password);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Registration failed.");
            return View(model);
        }

        await SignInUserAsync(result.User!, false);
        _logger.LogInformation("New user registered: {Email}", model.Email);
        return RedirectToLocal(model.ReturnUrl);
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _authService.ValidateCredentialsAsync(model.EmailOrMobile, model.Password);
        if (!result.Success)
        {
            // Surfaces the lockout message (with a wait time) when that's why it failed;
            // ValidateCredentialsAsync still returns the same generic "Invalid credentials." for
            // a wrong password, so this never distinguishes "wrong password" from "unknown user".
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Invalid email/mobile or password.");
            return View(model);
        }

        await SignInUserAsync(result.User!, model.RememberMe);
        return RedirectToLocal(model.ReturnUrl);
    }

    [HttpGet]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var (user, token) = await _authService.GeneratePasswordResetTokenAsync(model.Email);
        if (user is not null && token is not null)
        {
            var resetUrl = Url.Action(nameof(ResetPassword), "Account",
                new { userId = user.Id, token }, protocol: Request.Scheme)!;
            await _authMailer.SendPasswordResetAsync(user, resetUrl);
        }

        // Same confirmation either way — revealing whether the email matched an account would
        // let this form be used to enumerate registered guests.
        return View("ForgotPasswordConfirmation");
    }

    [HttpGet]
    public IActionResult ResetPassword(int userId, string token) =>
        View(new ResetPasswordViewModel { UserId = userId, Token = token });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _authService.ResetPasswordAsync(model.UserId, model.Token, model.NewPassword);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Password reset failed.");
            return View(model);
        }

        TempData["Success"] = "Your password has been reset. Please log in.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    public IActionResult AccessDenied() => View();

    private async Task SignInUserAsync(Models.Entities.User user, bool rememberMe)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = rememberMe });
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }
}
