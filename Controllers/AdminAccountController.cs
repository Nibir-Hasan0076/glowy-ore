using System.Security.Claims;
using GlowyOre.Data;
using GlowyOre.Services;
using GlowyOre.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlowyOre.Controllers;

[Route("admin")]
public class AdminAccountController : Controller
{
    private readonly AppDbContext _db;

    public AdminAccountController(AppDbContext db) => _db = db;

    [AllowAnonymous]
    [HttpGet("login")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return Redirect("/admin");
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var admin = await _db.AdminUsers.FirstOrDefaultAsync(a => a.Username == vm.Username.Trim());
        if (admin is null || !PasswordHasher.Verify(vm.Password, admin.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "That username or password doesn't match.");
            return View(vm);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, admin.Username),
            new(ClaimTypes.Role, "Admin"),
            new("DisplayName", admin.DisplayName ?? "Admin")
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true });

        TempData["Toast"] = $"Welcome back, {admin.DisplayName ?? admin.Username} ✨";

        if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
            return Redirect(vm.ReturnUrl);
        return Redirect("/admin");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/admin/login");
    }
}
