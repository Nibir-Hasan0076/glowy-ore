using GlowyOre.Data;
using GlowyOre.Models;
using GlowyOre.Services;
using GlowyOre.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlowyOre.Controllers;

[Authorize(Roles = "Admin")]
[Route("admin/products")]
public class AdminProductsController : Controller
{
    private readonly AppDbContext _db;
    private readonly ImageStorage _images;

    public AdminProductsController(AppDbContext db, ImageStorage images)
    {
        _db = db;
        _images = images;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? q)
    {
        var query = _db.Products.Include(p => p.Images).Include(p => p.Category).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(p => p.Name.Contains(term) ||
                                     (p.Category != null && p.Category.Name.Contains(term)));
        }
        ViewBag.Query = q;
        return View(await query.OrderByDescending(p => p.CreatedAt).ToListAsync());
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create()
    {
        return View(new ProductFormViewModel
        {
            Categories = await _db.Categories.OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToListAsync()
        });
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel vm)
    {
        if (!await CategoryExistsAsync(vm.CategoryId))
            ModelState.AddModelError(nameof(vm.CategoryId), "Please choose a category.");

        if (!ModelState.IsValid)
        {
            vm.Categories = await CategoriesAsync();
            return View(vm);
        }

        var product = new Product
        {
            Name = vm.Name.Trim(),
            Slug = await UniqueSlugAsync(vm.Name),
            Description = vm.Description?.Trim(),
            CategoryId = vm.CategoryId,
            Price = vm.Price,
            CompareAtPrice = vm.CompareAtPrice,
            Stock = vm.Stock,
            IsFeatured = vm.IsFeatured,
            IsActive = vm.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await AttachUploadsAsync(product, vm.Images);
        if (!ModelState.IsValid)
        {
            vm.Categories = await CategoriesAsync();
            return View(vm);
        }

        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        TempData["Toast"] = "Product created ✨";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _db.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();

        return View(new ProductFormViewModel
        {
            Id = product.Id,
            Name = product.Name,
            CategoryId = product.CategoryId,
            Description = product.Description,
            Price = product.Price,
            CompareAtPrice = product.CompareAtPrice,
            Stock = product.Stock,
            IsFeatured = product.IsFeatured,
            IsActive = product.IsActive,
            ExistingImages = product.Images.OrderBy(i => i.SortOrder).ToList(),
            Categories = await CategoriesAsync()
        });
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormViewModel vm)
    {
        var product = await _db.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();

        if (!await CategoryExistsAsync(vm.CategoryId))
            ModelState.AddModelError(nameof(vm.CategoryId), "Please choose a category.");

        if (!ModelState.IsValid)
        {
            vm.ExistingImages = product.Images.OrderBy(i => i.SortOrder).ToList();
            vm.Categories = await CategoriesAsync();
            return View(vm);
        }

        product.Name = vm.Name.Trim();
        product.Description = vm.Description?.Trim();
        product.CategoryId = vm.CategoryId;
        product.Price = vm.Price;
        product.CompareAtPrice = vm.CompareAtPrice;
        product.Stock = vm.Stock;
        product.IsFeatured = vm.IsFeatured;
        product.IsActive = vm.IsActive;

        foreach (var removeId in vm.RemoveImageIds)
        {
            var image = product.Images.FirstOrDefault(i => i.Id == removeId);
            if (image is null) continue;
            _images.Delete(image.ImagePath);
            _db.ProductImages.Remove(image);
        }

        var nextOrder = product.Images.Count == 0 ? 0 : product.Images.Max(i => i.SortOrder) + 1;
        await AttachUploadsAsync(product, vm.Images, nextOrder);
        if (!ModelState.IsValid)
        {
            vm.ExistingImages = product.Images.OrderBy(i => i.SortOrder).ToList();
            vm.Categories = await CategoriesAsync();
            return View(vm);
        }

        await _db.SaveChangesAsync();
        TempData["Toast"] = "Product updated ✨";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _db.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();

        foreach (var image in product.Images) _images.Delete(image.ImagePath);
        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        TempData["Toast"] = "Product deleted.";
        return RedirectToAction(nameof(Index));
    }

    private Task<List<Category>> CategoriesAsync() =>
        _db.Categories.OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToListAsync();

    private Task<bool> CategoryExistsAsync(int id) => _db.Categories.AnyAsync(c => c.Id == id);

    private async Task AttachUploadsAsync(Product product, List<IFormFile>? files, int startOrder = 0)
    {
        if (files is null) return;
        var order = startOrder;
        foreach (var file in files.Where(f => f.Length > 0))
        {
            try
            {
                var path = await _images.SaveAsync(file, "products");
                product.Images.Add(new ProductImage { ImagePath = path, SortOrder = order++ });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(ProductFormViewModel.Images), ex.Message);
            }
        }
    }

    private async Task<string> UniqueSlugAsync(string name)
    {
        var baseSlug = Slugger.Slugify(name);
        var slug = baseSlug;
        var n = 2;
        while (await _db.Products.AnyAsync(p => p.Slug == slug))
            slug = $"{baseSlug}-{n++}";
        return slug;
    }
}
