using System.ComponentModel.DataAnnotations;
using GlowyOre.Models;
using Microsoft.AspNetCore.Http;

namespace GlowyOre.ViewModels;

public class ProductCardViewModel
{
    public Product Product { get; set; } = null!;
}

public class HomeViewModel
{
    public List<Category> Categories { get; set; } = new();
    public List<Product> Featured { get; set; } = new();
}

public class CatalogViewModel
{
    public List<Product> Products { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public Category? ActiveCategory { get; set; }
    public string? Query { get; set; }
    public string? Sort { get; set; }
    public int TotalCount { get; set; }
}

public class CategoryPageViewModel
{
    public Category Category { get; set; } = null!;
    public List<Product> Products { get; set; } = new();
}

public class ProductPageViewModel
{
    public Product Product { get; set; } = null!;
    public List<Product> Related { get; set; } = new();
}

public class CartLine
{
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal LineTotal => Product.Price * Quantity;
}

public class CartViewModel
{
    public List<CartLine> Lines { get; set; } = new();
    public decimal Subtotal => Lines.Sum(l => l.LineTotal);
    public decimal DeliveryFee => Lines.Count == 0 || Subtotal >= 2000 ? 0 : 80;
    public decimal Total => Subtotal + DeliveryFee;
}

public class CheckoutViewModel
{
    [Required(ErrorMessage = "Please enter your name")]
    [StringLength(120)]
    [Display(Name = "Full name")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your phone number")]
    [StringLength(30, MinimumLength = 6, ErrorMessage = "Enter a valid phone number")]
    [Display(Name = "Phone number")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your delivery address")]
    [StringLength(400)]
    [Display(Name = "Delivery address")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your city")]
    [StringLength(80)]
    public string City { get; set; } = string.Empty;

    [StringLength(600)]
    [Display(Name = "Order note (optional)")]
    public string? Note { get; set; }

    public CartViewModel Cart { get; set; } = new();
}

public class LoginViewModel
{
    [Required(ErrorMessage = "Enter your username")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter your password")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

public class CategoryFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Category name is required")]
    [StringLength(80)]
    [Display(Name = "Category name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Description { get; set; }

    [Display(Name = "Display order")]
    public int SortOrder { get; set; }

    public List<IFormFile>? Images { get; set; }
    public List<CategoryImage> ExistingImages { get; set; } = new();
    public List<int> RemoveImageIds { get; set; } = new();
}

public class ProductFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Product name is required")]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Choose a category")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Price is required")]
    [Range(1, 1000000, ErrorMessage = "Price must be at least ৳1")]
    [Display(Name = "Price (৳)")]
    public decimal Price { get; set; }

    [Range(0, 1000000, ErrorMessage = "Compare price cannot be negative")]
    [Display(Name = "Compare-at price (৳, optional)")]
    public decimal? CompareAtPrice { get; set; }

    [Range(0, 100000, ErrorMessage = "Stock cannot be negative")]
    public int Stock { get; set; }

    [Display(Name = "Feature on home page")]
    public bool IsFeatured { get; set; }

    [Display(Name = "Active / visible in store")]
    public bool IsActive { get; set; } = true;

    public List<IFormFile>? Images { get; set; }
    public List<ProductImage> ExistingImages { get; set; } = new();
    public List<int> RemoveImageIds { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
}

public class AdminDashboardViewModel
{
    public int OrderCount { get; set; }
    public int PendingCount { get; set; }
    public int ProductCount { get; set; }
    public int CategoryCount { get; set; }
    public decimal Revenue { get; set; }
    public List<Order> RecentOrders { get; set; } = new();
    public List<Product> LowStock { get; set; } = new();
}

public class AdminOrderViewModel
{
    public Order Order { get; set; } = null!;
}
