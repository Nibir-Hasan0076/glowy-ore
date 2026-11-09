using System.Text.Json;
using GlowyOre.Data;
using GlowyOre.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GlowyOre.Services;

public class CartItem
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class CartService
{
    private const string SessionKey = "glowy_cart";
    private readonly IHttpContextAccessor _http;

    public CartService(IHttpContextAccessor http) => _http = http;

    public List<CartItem> Get()
    {
        var raw = _http.HttpContext?.Session.GetString(SessionKey);
        if (string.IsNullOrEmpty(raw)) return new List<CartItem>();
        try
        {
            return JsonSerializer.Deserialize<List<CartItem>>(raw) ?? new List<CartItem>();
        }
        catch (JsonException)
        {
            return new List<CartItem>();
        }
    }

    public void Save(List<CartItem> items)
    {
        _http.HttpContext?.Session.SetString(SessionKey, JsonSerializer.Serialize(items));
    }

    public void Add(int productId, int quantity = 1)
    {
        var items = Get();
        var existing = items.FirstOrDefault(i => i.ProductId == productId);
        if (existing is null)
            items.Add(new CartItem { ProductId = productId, Quantity = Math.Clamp(quantity, 1, 99) });
        else
            existing.Quantity = Math.Clamp(existing.Quantity + quantity, 1, 99);
        Save(items);
    }

    public void Update(int productId, int quantity)
    {
        var items = Get();
        var existing = items.FirstOrDefault(i => i.ProductId == productId);
        if (existing is null) return;
        if (quantity <= 0) items.Remove(existing);
        else existing.Quantity = Math.Min(quantity, 99);
        Save(items);
    }

    public void Remove(int productId)
    {
        var items = Get();
        items.RemoveAll(i => i.ProductId == productId);
        Save(items);
    }

    public void Clear()
    {
        _http.HttpContext?.Session.Remove(SessionKey);
    }

    public int Count => Get().Sum(i => i.Quantity);

    public async Task<CartViewModel> BuildViewModelAsync(AppDbContext db, bool includeInactive = false)
    {
        var items = Get();
        var ids = items.Select(i => i.ProductId).ToList();
        var products = await db.Products.Include(p => p.Images)
            .Where(p => ids.Contains(p.Id) && (includeInactive || p.IsActive))
            .ToListAsync();

        var lines = new List<CartLine>();
        foreach (var item in items)
        {
            var product = products.FirstOrDefault(p => p.Id == item.ProductId);
            if (product is null) continue;
            lines.Add(new CartLine { Product = product, Quantity = item.Quantity });
        }

        return new CartViewModel { Lines = lines };
    }
}
