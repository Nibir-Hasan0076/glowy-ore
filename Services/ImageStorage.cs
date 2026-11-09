namespace GlowyOre.Services;

public class ImageStorage
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    private const long MaxBytes = 6 * 1024 * 1024;
    private readonly string _root;

    public ImageStorage(IWebHostEnvironment env, IConfiguration config)
    {
        var configured = config["Storage:UploadsRoot"];
        _root = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(env.WebRootPath, "uploads")
            : Path.GetFullPath(configured);
    }

    public async Task<string> SaveAsync(IFormFile file, string folder)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            throw new InvalidOperationException($"Unsupported image type: {ext}");
        if (file.Length == 0 || file.Length > MaxBytes)
            throw new InvalidOperationException("Image must be between 1 byte and 6 MB.");

        var absoluteDir = Path.Combine(_root, folder);
        Directory.CreateDirectory(absoluteDir);

        var name = $"{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}{ext}";
        var absolutePath = Path.Combine(absoluteDir, name);
        await using var stream = new FileStream(absolutePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"uploads/{folder}/{name}";
    }

    public void Delete(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return;

        var relative = relativePath.Replace('\\', '/');
        const string prefix = "uploads/";
        if (relative.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            relative = relative[prefix.Length..];

        var root = Path.GetFullPath(_root);
        var full = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
        if (!full.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) return;
        if (File.Exists(full)) File.Delete(full);
    }
}
