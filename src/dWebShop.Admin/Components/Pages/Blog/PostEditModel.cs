using System.Globalization;
using System.Text;

namespace dWebShop.Admin.Components.Pages.Blog;

public class PostEditModel
{
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Content { get; set; } = "";
    public string MetaTitle { get; set; } = "";
    public string MetaDescription { get; set; } = "";
    public bool Published { get; set; }
    public HashSet<int> CategoryIds { get; set; } = new();
    public HashSet<int> TagIds { get; set; } = new();
}

public static class SlugHelper
{
    public static string Slugify(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";

        // Map common Bosnian/Croatian diacritics before normalizing.
        value = value
            .Replace("č", "c").Replace("ć", "c").Replace("đ", "d").Replace("š", "s").Replace("ž", "z")
            .Replace("Č", "c").Replace("Ć", "c").Replace("Đ", "d").Replace("Š", "s").Replace("Ž", "z");

        var normalized = value.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
            if (char.IsLetterOrDigit(c)) sb.Append(char.ToLowerInvariant(c));
            else if (char.IsWhiteSpace(c) || c is '-' or '_') sb.Append('-');
        }

        var slug = sb.ToString();
        while (slug.Contains("--")) slug = slug.Replace("--", "-");
        return slug.Trim('-');
    }
}
