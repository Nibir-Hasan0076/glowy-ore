using System.ComponentModel.DataAnnotations;

namespace GlowyOre.Models;

public class Product
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(140)]
    public string Slug { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }

    public int Stock { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public string? PrimaryImage => Images.OrderBy(i => i.SortOrder).Select(i => i.ImagePath).FirstOrDefault();
    public bool HasDiscount => CompareAtPrice.HasValue && CompareAtPrice.Value > Price;
}

public class ProductImage
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    [Required]
    public string ImagePath { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}
