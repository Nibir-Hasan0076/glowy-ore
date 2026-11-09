using GlowyOre.Data;
using GlowyOre.Models;
using GlowyOre.Services;
using GlowyOre.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlowyOre.Controllers;

public class CheckoutController : Controller
{
    private readonly AppDbContext _db;
    private readonly CartService _cart;

    public CheckoutController(AppDbContext db, CartService cart)
    {
        _db = db;
        _cart = cart;
    }

    [HttpGet("/checkout")]
    public async Task<IActionResult> Index()
    {
        var cart = await _cart.BuildViewModelAsync(_db);
        if (cart.Lines.Count == 0)
            return RedirectToAction("Index", "Cart");

        return View(new CheckoutViewModel { Cart = cart });
    }

    [HttpPost("/checkout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(CheckoutViewModel vm)
    {
        var cart = await _cart.BuildViewModelAsync(_db);
        if (cart.Lines.Count == 0)
        {
            TempData["Toast"] = "Your bag is empty.";
            return RedirectToAction("Index", "Cart");
        }

        vm.Cart = cart;

        foreach (var line in cart.Lines)
        {
            if (line.Quantity > line.Product.Stock)
                ModelState.AddModelError(string.Empty,
                    $"Only {line.Product.Stock} of {line.Product.Name} left in stock.");
        }

        if (!ModelState.IsValid) return View("Index", vm);

        var order = new Order
        {
            CustomerName = vm.CustomerName.Trim(),
            Phone = vm.Phone.Trim(),
            Address = vm.Address.Trim(),
            City = vm.City.Trim(),
            Note = string.IsNullOrWhiteSpace(vm.Note) ? null : vm.Note.Trim(),
            Subtotal = cart.Subtotal,
            DeliveryFee = cart.DeliveryFee,
            Total = cart.Total,
            Status = OrderStatuses.Pending,
            PaymentMethod = "Cash on Delivery",
            CreatedAt = DateTime.UtcNow,
            Items = cart.Lines.Select(l => new OrderItem
            {
                ProductId = l.Product.Id,
                ProductName = l.Product.Name,
                ImagePath = l.Product.PrimaryImage,
                UnitPrice = l.Product.Price,
                Quantity = l.Quantity
            }).ToList()
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        order.OrderNumber = $"GO-{DateTime.Now:yyyyMMdd}-{order.Id:D4}";

        foreach (var line in cart.Lines)
        {
            var product = await _db.Products.FindAsync(line.Product.Id);
            if (product is not null)
                product.Stock = Math.Max(0, product.Stock - line.Quantity);
        }

        await _db.SaveChangesAsync();
        _cart.Clear();

        return RedirectToAction(nameof(Success), new { id = order.Id });
    }

    [HttpGet("/checkout/success/{id:int}")]
    public async Task<IActionResult> Success(int id)
    {
        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return NotFound();
        return View(order);
    }
}
