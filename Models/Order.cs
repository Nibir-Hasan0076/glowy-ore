using System.ComponentModel.DataAnnotations;

namespace GlowyOre.Models;

public class Order
{
    public int Id { get; set; }

    [Required, StringLength(40)]
    public string OrderNumber { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string CustomerName { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string Phone { get; set; } = string.Empty;

    [Required, StringLength(400)]
    public string Address { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string City { get; set; } = string.Empty;

    [StringLength(600)]
    public string? Note { get; set; }

    public decimal Subtotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal Total { get; set; }

    [Required, StringLength(30)]
    public string Status { get; set; } = OrderStatuses.Pending;

    [Required, StringLength(30)]
    public string PaymentMethod { get; set; } = "Cash on Delivery";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    [Required, StringLength(120)]
    public string ProductName { get; set; } = string.Empty;

    public string? ImagePath { get; set; }

    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
}

public static class OrderStatuses
{
    public const string Pending = "Pending";
    public const string Confirmed = "Confirmed";
    public const string Shipped = "Shipped";
    public const string Delivered = "Delivered";
    public const string Cancelled = "Cancelled";

    public static readonly string[] All = { Pending, Confirmed, Shipped, Delivered, Cancelled };
}
