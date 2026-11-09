using GlowyOre.Data;
using GlowyOre.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlowyOre.Controllers;

public class ShopController : Controller
{
    private readonly AppDbContext _db;

    public ShopController(AppDbContext db) => _db = db;

    [HttpGet("/shop")]
    public async Task<IActionResult> Index(string? q, string? sort, string? category)
    {
        var categories = await _db.Categories.Include(c => c.Images)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToListAsync();

        var query = _db.Products.Include(p => p.Images).Include(p => p.Category)
            .Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category != null && p.Category.Slug == category);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(p => p.Name.Contains(term) ||
                                     (p.Description != null && p.Description.Contains(term)));
        }

        query = sort switch
        {
            "price-low" => query.OrderBy(p => p.Price),
            "price-high" => query.OrderByDescending(p => p.Price),
            "name" => query.OrderBy(p => p.Name),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var products = await query.ToListAsync();

        return View(new CatalogViewModel
        {
            Products = products,
            Categories = categories,
            ActiveCategory = categories.FirstOrDefault(c => c.Slug == category),
            Query = q,
            Sort = sort,
            TotalCount = products.Count
        });
    }

    [HttpGet("/category/{slug}")]
    public async Task<IActionResult> Category(string slug)
    {
        var category = await _db.Categories
            .Include(c => c.Images)
            .FirstOrDefaultAsync(c => c.Slug == slug);
        if (category is null) return NotFound();

        var products = await _db.Products
            .Include(p => p.Images)
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.CategoryId == category.Id)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return View(new CategoryPageViewModel { Category = category, Products = products });
    }

    [HttpGet("/product/{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        var product = await _db.Products
            .Include(p => p.Images)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);
        if (product is null) return NotFound();

        var related = await _db.Products
            .Include(p => p.Images)
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.CategoryId == product.CategoryId && p.Id != product.Id)
            .OrderByDescending(p => p.CreatedAt)
            .Take(4)
            .ToListAsync();

        return View(new ProductPageViewModel { Product = product, Related = related });
    }
}
