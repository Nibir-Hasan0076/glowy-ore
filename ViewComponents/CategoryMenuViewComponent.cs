using GlowyOre.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlowyOre.ViewComponents;

public class CategoryMenuViewComponent : ViewComponent
{
    private readonly AppDbContext _db;

    public CategoryMenuViewComponent(AppDbContext db) => _db = db;

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var categories = await _db.Categories
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .Take(8)
            .ToListAsync();
        return View(categories);
    }
}
