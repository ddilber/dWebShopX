using dWebShop.Domain.Common;

namespace dWebShop.Domain.Entities.Social;

/// <summary>
/// One social network the company links to from the homepage "Društvene mreže"
/// block.
/// </summary>
public class SocialProfile : BaseAuditableEntity
{
    /// <summary>
    /// Selects the inline icon on the public page — "facebook", "instagram" or
    /// "tiktok". An unrecognised key still renders, with a generic link icon.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Network name shown on the card, e.g. "Instagram".</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Printed under the name, e.g. "@asgifiks" or "#asgifiks".</summary>
    public string Handle { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool Published { get; set; } = true;
}
