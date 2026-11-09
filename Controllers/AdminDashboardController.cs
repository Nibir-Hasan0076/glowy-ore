using GlowyOre.Data;
using GlowyOre.Models;
using GlowyOre.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlowyOre.Controllers;

[Authorize(Roles = "Admin")]
[Route("admin")]
public class AdminDashboardController : Controller
{
    private readonly AppDbContext _db;

    public AdminDashboardController(AppDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var orders = await _db.Orders.Include(o => o.Items).ToListAsync();
        var vm = new AdminDashboardViewModel
        {
            OrderCount = orders.Count,
            PendingCount = orders.Count(o => o.Status == OrderStatuses.Pending),
            Revenue = orders.Where(o => o.Status != OrderStatuses.Cancelled).Sum(o => o.Total),
            ProductCount = await _db.Products.CountAsync(),
            CategoryCount = await _db.Categories.CountAsync(),
            RecentOrders = await _db.Orders.Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt).Take(6).ToListAsync(),
            LowStock = await _db.Products.Include(p => p.Images).Include(p => p.Category)
                .Where(p => p.IsActive && p.Stock <= 5)
                .OrderBy(p => p.Stock).Take(6).ToListAsync()
        };
        return View(vm);
    }
}
