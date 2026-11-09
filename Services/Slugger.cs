namespace GlowyOre.Services;

public static class Slugger
{
    public static string Slugify(string input)
    {
        var cleaned = new string(input.Trim().ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray());
        while (cleaned.Contains("--")) cleaned = cleaned.Replace("--", "-");
        cleaned = cleaned.Trim('-');
        return string.IsNullOrWhiteSpace(cleaned) ? "item" : cleaned;
    }
}
