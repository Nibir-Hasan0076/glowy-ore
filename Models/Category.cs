using System.ComponentModel.DataAnnotations;

namespace GlowyOre.Models;

public class Category
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Slug { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public ICollection<CategoryImage> Images { get; set; } = new List<CategoryImage>();
    public ICollection<Product> Products { get; set; } = new List<Product>();

    public string? IconPath => Images.OrderBy(i => i.SortOrder).Select(i => i.ImagePath).FirstOrDefault();
}

public class CategoryImage
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    [Required]
    public string ImagePath { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}
