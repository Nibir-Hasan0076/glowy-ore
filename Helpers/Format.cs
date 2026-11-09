using System.Globalization;

namespace GlowyOre.Helpers;

public static class Format
{
    public static string Money(decimal value) => "৳" + value.ToString("N0", CultureInfo.InvariantCulture);

    public static string Money(decimal? value) => value.HasValue ? Money(value.Value) : string.Empty;

    public static string DateTimeLocal(DateTime utc) =>
        DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToLocalTime().ToString("dd MMM yyyy, h:mm tt");

    public static string DateLocal(DateTime utc) =>
        DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToLocalTime().ToString("dd MMM yyyy");

    public static string Initials(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 0 ? "?" : string.Concat(parts.Take(2).Select(p => char.ToUpperInvariant(p[0])));
    }
}
