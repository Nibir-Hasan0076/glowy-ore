using GlowyOre.Data;
using GlowyOre.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlowyOre.Controllers;

[Authorize(Roles = "Admin")]
[Route("admin/orders")]
public class AdminOrdersController : Controller
{
    private readonly AppDbContext _db;

    public AdminOrdersController(AppDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index(string? status)
    {
        var query = _db.Orders.Include(o => o.Items).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status) && OrderStatuses.All.Contains(status))
            query = query.Where(o => o.Status == status);

        ViewBag.Status = status;
        ViewBag.Statuses = OrderStatuses.All;
        return View(await query.OrderByDescending(o => o.CreatedAt).ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var order = await _db.Orders.Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return NotFound();

        ViewBag.Statuses = OrderStatuses.All;
        return View(order);
    }

    [HttpPost("{id:int}/status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order is null) return NotFound();

        if (!OrderStatuses.All.Contains(status))
        {
            TempData["Toast"] = "That order status isn't valid.";
            return RedirectToAction(nameof(Details), new { id });
        }

        order.Status = status;
        await _db.SaveChangesAsync();
        TempData["Toast"] = $"Order marked as {status} ✨";
        return RedirectToAction(nameof(Details), new { id });
    }
}
