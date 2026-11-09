using GlowyOre.Data;
using GlowyOre.Services;
using GlowyOre.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlowyOre.Controllers;

public class CartController : Controller
{
    private readonly AppDbContext _db;
    private readonly CartService _cart;

    public CartController(AppDbContext db, CartService cart)
    {
        _db = db;
        _cart = cart;
    }

    [HttpGet("/cart")]
    public async Task<IActionResult> Index()
    {
        return View(await _cart.BuildViewModelAsync(_db));
    }

    [HttpPost("/cart/add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int id, int qty = 1, string? returnUrl = null)
    {
        var stock = await _db.Products
            .Where(p => p.Id == id && p.IsActive)
            .Select(p => (int?)p.Stock)
            .FirstOrDefaultAsync();

        if (stock is > 0)
        {
            var cap = Math.Min(99, stock.Value);
            _cart.Add(id, Math.Clamp(qty, 1, cap));
            var line = _cart.Get().FirstOrDefault(i => i.ProductId == id);
            if (line is not null && line.Quantity > cap) _cart.Update(id, cap);
            TempData["Toast"] = "Added to your bag ✨";
        }
        else
        {
            TempData["Toast"] = "That piece is sold out.";
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/cart/update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, int qty)
    {
        var stock = await _db.Products.Where(p => p.Id == id).Select(p => p.Stock).FirstOrDefaultAsync();
        if (stock > 0) _cart.Update(id, Math.Clamp(qty, 0, stock));
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/cart/remove")]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int id)
    {
        _cart.Remove(id);
        return RedirectToAction(nameof(Index));
    }
}
