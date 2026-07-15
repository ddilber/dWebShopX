using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;

namespace dWebShop.Web;

public static partial class Seo
{
    public const string BaseUrl = "https://asgifiks.ba";

    [GeneratedRegex("<[^>]+>", RegexOptions.Singleline)]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    // Flattens stored HTML (product/post bodies) to plain text. Meta descriptions
    // and JSON-LD must never contain markup, so anything rendered as HTML on the
    // page has to go through this before it reaches <meta> or structured data.
    public static string PlainText(string? html, int maxLength = 300)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;

        // Block-level tags become spaces so words don't run together.
        var text = HtmlTagRegex().Replace(html, " ");
        text = System.Net.WebUtility.HtmlDecode(text);
        text = WhitespaceRegex().Replace(text, " ").Trim();

        if (maxLength > 0 && text.Length > maxLength)
            text = text[..maxLength].TrimEnd() + "…";

        return text;
    }

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };

    public static string Canonical(string path)
    {
        if (string.IsNullOrEmpty(path)) return BaseUrl + "/";
        return path.StartsWith("http") ? path : BaseUrl + (path.StartsWith('/') ? path : "/" + path);
    }

    // Absolute URL to the JPEG rendition of a stored image, served by the /og
    // endpoint (social crawlers don't render the source WebP reliably). A null
    // or empty path resolves to the site-wide default image.
    public static string OgImage(string? storedPath)
    {
        var clean = string.IsNullOrWhiteSpace(storedPath)
            ? "images/asgifiks_building.jpeg"
            : storedPath.Trim().TrimStart('/');
        return $"{BaseUrl}/og/{clean}";
    }

    public static string DefaultImage => OgImage(null);

    // Emits the full og:image tag set for a stored image path. The /og endpoint
    // always returns a 1200x630 JPEG, so the declared dimensions are accurate.
    public static MarkupString OgImageTags(string? storedPath, string? alt = null)
    {
        var url = System.Net.WebUtility.HtmlEncode(OgImage(storedPath));
        var a = System.Net.WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(alt) ? "A&S GIFIKS" : alt);
        return new MarkupString(
            $"<meta property=\"og:image\" content=\"{url}\" />" +
            $"<meta property=\"og:image:secure_url\" content=\"{url}\" />" +
            $"<meta property=\"og:image:type\" content=\"image/jpeg\" />" +
            $"<meta property=\"og:image:width\" content=\"1200\" />" +
            $"<meta property=\"og:image:height\" content=\"630\" />" +
            $"<meta property=\"og:image:alt\" content=\"{a}\" />");
    }

    public static MarkupString JsonLd(JsonObject obj)
    {
        obj["@context"] ??= "https://schema.org";
        return new MarkupString($"<script type=\"application/ld+json\">{obj.ToJsonString(JsonOpts)}</script>");
    }
}
