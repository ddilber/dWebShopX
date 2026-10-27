using dWebShop.Domain.Common;

namespace dWebShop.Domain.Entities.Social;

/// <summary>
/// A hand-picked social video shown in the homepage "Društvene mreže" block.
///
/// The public page renders these as a facade: only the poster is shown until
/// the visitor clicks, so nothing is requested from TikTok/Instagram — and no
/// third-party cookie is set — on page load.
/// </summary>
public class SocialVideo : BaseAuditableEntity
{
    /// <summary>Label shown top-left on the poster, e.g. "TikTok".</summary>
    public string Platform { get; set; } = string.Empty;

    public string Caption { get; set; } = string.Empty;

    /// <summary>Free text, e.g. "0:42". Shown in the card footer.</summary>
    public string Duration { get; set; } = string.Empty;

    /// <summary>Still shown before any click. Empty falls back to a placeholder.</summary>
    public string PosterImage { get; set; } = string.Empty;

    /// <summary>
    /// The platform's *embed* URL, not the page URL:
    /// TikTok <c>https://www.tiktok.com/embed/v2/{videoId}</c>,
    /// Instagram <c>https://www.instagram.com/reel/{code}/embed</c>.
    /// Leave empty and the card links out to <see cref="FallbackUrl"/> instead
    /// of embedding.
    /// </summary>
    public string EmbedUrl { get; set; } = string.Empty;

    /// <summary>Where the card points when there is no embed URL.</summary>
    public string FallbackUrl { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool Published { get; set; } = true;
}
