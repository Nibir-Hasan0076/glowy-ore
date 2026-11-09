using System.ComponentModel.DataAnnotations;

namespace GlowyOre.Models;

public class AdminUser
{
    public int Id { get; set; }

    [Required, StringLength(60)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [StringLength(80)]
    public string? DisplayName { get; set; }
}
