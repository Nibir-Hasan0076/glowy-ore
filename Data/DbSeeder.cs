using System.Security.Cryptography;
using GlowyOre.Models;
using GlowyOre.Services;
using Microsoft.EntityFrameworkCore;

namespace GlowyOre.Data;

public static class DbSeeder
{
    public const string AdminUsername = "admin";

    public static string? Seed(AppDbContext db, string? adminPassword)
    {
        string? generatedPassword = null;

        var admin = db.AdminUsers.FirstOrDefault(u => u.Username == AdminUsername);
        if (admin is null)
        {
            var password = adminPassword;
            if (string.IsNullOrWhiteSpace(password))
            {
                password = GeneratePassword();
                generatedPassword = password;
            }

            db.AdminUsers.Add(new AdminUser
            {
                Username = AdminUsername,
                PasswordHash = PasswordHasher.Hash(password),
                DisplayName = "GLOWY ORE Admin"
            });
            db.SaveChanges();
        }
        else if (!string.IsNullOrWhiteSpace(adminPassword) &&
                 !PasswordHasher.Verify(adminPassword, admin.PasswordHash))
        {
            admin.PasswordHash = PasswordHasher.Hash(adminPassword);
            db.SaveChanges();
        }

        if (db.Categories.Any()) return generatedPassword;

        var necklaces = new Category
        {
            Name = "Necklaces",
            Slug = "necklaces",
            Description = "Layered chains, pearl pendants and everything that sits close to the heart.",
            SortOrder = 1
        };
        var earrings = new Category
        {
            Name = "Earrings",
            Slug = "earrings",
            Description = "Drops, studs and statement sparkle to catch every little light.",
            SortOrder = 2
        };
        var rings = new Category
        {
            Name = "Rings",
            Slug = "rings",
            Description = "Delicate bands made for stacking, gifting and everyday glow.",
            SortOrder = 3
        };
        var bracelets = new Category
        {
            Name = "Bracelets",
            Slug = "bracelets",
            Description = "Charm bracelets and pearly bangles that follow your every gesture.",
            SortOrder = 4
        };

        db.Categories.AddRange(necklaces, earrings, rings, bracelets);

        db.CategoryImages.AddRange(
            new CategoryImage { Category = necklaces, ImagePath = "images/seed/category-necklaces.jpg", SortOrder = 0 },
            new CategoryImage { Category = earrings, ImagePath = "images/seed/category-earrings.jpg", SortOrder = 0 },
            new CategoryImage { Category = rings, ImagePath = "images/seed/category-rings.jpg", SortOrder = 0 },
            new CategoryImage { Category = bracelets, ImagePath = "images/seed/category-bracelets.jpg", SortOrder = 0 });

        void AddProduct(Product product, string image)
        {
            db.Products.Add(product);
            db.ProductImages.Add(new ProductImage { Product = product, ImagePath = image, SortOrder = 0 });
        }

        AddProduct(new Product
        {
            Name = "Aurora Pearl Pendant",
            Slug = "aurora-pearl-pendant",
            Category = necklaces,
            Price = 1850,
            CompareAtPrice = 2200,
            Stock = 12,
            IsFeatured = true,
            CreatedAt = DateTime.UtcNow.AddDays(-24),
            Description = "A single freshwater pearl suspended from a whisper-thin gold chain. The Aurora catches the light with every turn, making even a plain tee feel like an occasion."
        }, "images/seed/product-aurora-pendant.jpg");

        AddProduct(new Product
        {
            Name = "Rosé Bloom Layered Necklace",
            Slug = "rose-bloom-layered-necklace",
            Category = necklaces,
            Price = 1650,
            Stock = 9,
            IsFeatured = true,
            CreatedAt = DateTime.UtcNow.AddDays(-9),
            Description = "Two delicate chains, one tiny rose charm. Designed to sit perfectly layered so you never have to untangle your favourites again."
        }, "images/seed/product-rose-bloom-necklace.jpg");

        AddProduct(new Product
        {
            Name = "Blush Petal Drop Earrings",
            Slug = "blush-petal-drop-earrings",
            Category = earrings,
            Price = 950,
            CompareAtPrice = 1200,
            Stock = 20,
            IsFeatured = true,
            CreatedAt = DateTime.UtcNow.AddDays(-18),
            Description = "Hand-painted enamel petals with a soft crystal centre. Feather-light, so you can wear them from morning coffee to dinner reservations."
        }, "images/seed/product-blush-petal-earrings.jpg");

        AddProduct(new Product
        {
            Name = "Crystal Whisper Studs",
            Slug = "crystal-whisper-studs",
            Category = earrings,
            Price = 750,
            Stock = 25,
            IsFeatured = false,
            CreatedAt = DateTime.UtcNow.AddDays(-31),
            Description = "The quiet kind of sparkle. Tiny crystals set in gold-tone brass — the studs you will reach for every single day."
        }, "images/seed/product-crystal-whisper-studs.jpg");

        AddProduct(new Product
        {
            Name = "Velvet Rose Ring",
            Slug = "velvet-rose-ring",
            Category = rings,
            Price = 1250,
            Stock = 15,
            IsFeatured = true,
            CreatedAt = DateTime.UtcNow.AddDays(-14),
            Description = "A sculpted rose in a soft rose-gold finish, sized to be stacked or worn alone. A little romance for your hand."
        }, "images/seed/product-velvet-rose-ring.jpg");

        AddProduct(new Product
        {
            Name = "Golden Halo Ring",
            Slug = "golden-halo-ring",
            Category = rings,
            Price = 1450,
            CompareAtPrice = 1700,
            Stock = 10,
            IsFeatured = false,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            Description = "A halo of pavé stones around a single pearl. Classic, luminous and endlessly flattering on every hand."
        }, "images/seed/product-golden-halo-ring.jpg");

        AddProduct(new Product
        {
            Name = "Dainty Charm Bracelet",
            Slug = "dainty-charm-bracelet",
            Category = bracelets,
            Price = 1150,
            Stock = 18,
            IsFeatured = false,
            CreatedAt = DateTime.UtcNow.AddDays(-7),
            Description = "A fine chain trimmed with tiny hearts and a single pearl drop. Adjustable, giftable and made to be layered."
        }, "images/seed/product-dainty-charm-bracelet.jpg");

        AddProduct(new Product
        {
            Name = "Pink Pearl Bangle",
            Slug = "pink-pearl-bangle",
            Category = bracelets,
            Price = 1350,
            Stock = 14,
            IsFeatured = false,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            Description = "A smooth blush bangle scattered with tiny seed pearls. The easiest way to add a little GLOW to your wrist."
        }, "images/seed/product-pink-pearl-bangle.jpg");

        db.SaveChanges();
        return generatedPassword;
    }

    private static string GeneratePassword()
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
        return new string(Enumerable.Range(0, 20)
            .Select(_ => alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)])
            .ToArray());
    }
}
