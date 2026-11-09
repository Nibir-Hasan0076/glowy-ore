using GlowyOre.Data;
using GlowyOre.Models;
using GlowyOre.Services;
using GlowyOre.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlowyOre.Controllers;

[Authorize(Roles = "Admin")]
[Route("admin/categories")]
public class AdminCategoriesController : Controller
{
    private readonly AppDbContext _db;
    private readonly ImageStorage _images;

    public AdminCategoriesController(AppDbContext db, ImageStorage images)
    {
        _db = db;
        _images = images;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var categories = await _db.Categories
            .Include(c => c.Images)
            .Include(c => c.Products)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .ToListAsync();
        return View(categories);
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        return View(new CategoryFormViewModel());
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var category = new Category
        {
            Name = vm.Name.Trim(),
            Slug = await UniqueSlugAsync(vm.Name),
            Description = vm.Description?.Trim(),
            SortOrder = vm.SortOrder
        };

        await AttachUploadsAsync(category, vm.Images);
        if (!ModelState.IsValid) return View(vm);

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        TempData["Toast"] = "Category created ✨";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _db.Categories.Include(c => c.Images).FirstOrDefaultAsync(c => c.Id == id);
        if (category is null) return NotFound();

        return View(new CategoryFormViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            SortOrder = category.SortOrder,
            ExistingImages = category.Images.OrderBy(i => i.SortOrder).ToList()
        });
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryFormViewModel vm)
    {
        var category = await _db.Categories.Include(c => c.Images).FirstOrDefaultAsync(c => c.Id == id);
        if (category is null) return NotFound();
        if (!ModelState.IsValid)
        {
            vm.ExistingImages = category.Images.OrderBy(i => i.SortOrder).ToList();
            return View(vm);
        }

        category.Name = vm.Name.Trim();
        category.Description = vm.Description?.Trim();
        category.SortOrder = vm.SortOrder;

        foreach (var removeId in vm.RemoveImageIds)
        {
            var image = category.Images.FirstOrDefault(i => i.Id == removeId);
            if (image is null) continue;
            _images.Delete(image.ImagePath);
            _db.CategoryImages.Remove(image);
        }

        var nextOrder = category.Images.Count == 0 ? 0 : category.Images.Max(i => i.SortOrder) + 1;
        await AttachUploadsAsync(category, vm.Images, nextOrder);
        if (!ModelState.IsValid)
        {
            vm.ExistingImages = category.Images.OrderBy(i => i.SortOrder).ToList();
            return View(vm);
        }

        await _db.SaveChangesAsync();
        TempData["Toast"] = "Category updated ✨";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.Categories.Include(c => c.Images).Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (category is null) return NotFound();

        if (category.Products.Any())
        {
            TempData["Toast"] = "Move or delete this category's products first.";
            return RedirectToAction(nameof(Index));
        }

        foreach (var image in category.Images) _images.Delete(image.ImagePath);
        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        TempData["Toast"] = "Category deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task AttachUploadsAsync(Category category, List<IFormFile>? files, int startOrder = 0)
    {
        if (files is null) return;
        var order = startOrder;
        foreach (var file in files.Where(f => f.Length > 0))
        {
            try
            {
                var path = await _images.SaveAsync(file, "categories");
                category.Images.Add(new CategoryImage { ImagePath = path, SortOrder = order++ });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(CategoryFormViewModel.Images), ex.Message);
            }
        }
    }

    private async Task<string> UniqueSlugAsync(string name)
    {
        var baseSlug = Slugger.Slugify(name);
        var slug = baseSlug;
        var n = 2;
        while (await _db.Categories.AnyAsync(c => c.Slug == slug))
            slug = $"{baseSlug}-{n++}";
        return slug;
    }
}
