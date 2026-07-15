using System.Text.RegularExpressions;

namespace dWebShop.Web.Services;

// View models the shop razor pages bind to. Mirrors the legacy ShopData record
// shape on purpose so the page markup needs only the smallest possible diff
// when switching the data source from static arrays to MediatR.

public record ShopBrandVm(int Id, string Slug, string Name, string Tagline, string Origin, string Since);

// Hierarchical category — Children carries any direct sub-categories.
// Empty Children means a leaf; absent ParentCategoryId means a root.
// The shape mirrors the Category entity's self-referential parent/child model.
public record ShopCategoryVm(
    int Id,
    string Slug,
    string Name,
    string Description,
    int? ParentCategoryId,
    IReadOnlyList<ShopCategoryVm> Children);

public static class ShopCategoryExtensions
{
    // Depth-first traversal yielding each node with its depth in the tree.
    // Root nodes start at depth 0 — pages use this for indentation styling.
    public static IEnumerable<(ShopCategoryVm Category, int Depth)> Flatten(
        this IEnumerable<ShopCategoryVm> roots,
        int depth = 0)
    {
        foreach (var c in roots)
        {
            yield return (c, depth);
            foreach (var d in c.Children.Flatten(depth + 1))
                yield return d;
        }
    }

    // Recursive slug lookup — finds a category at any depth under the
    // supplied roots. Slug is the URL identity, so this resolves the
    // `/shop/{brand}/{cat}` route regardless of where in the hierarchy it
    // sits.
    public static ShopCategoryVm? FindBySlug(this IEnumerable<ShopCategoryVm> roots, string? slug)
    {
        if (string.IsNullOrEmpty(slug)) return null;
        foreach (var c in roots)
        {
            if (string.Equals(c.Slug, slug, StringComparison.OrdinalIgnoreCase))
                return c;
            var found = c.Children.FindBySlug(slug);
            if (found is not null) return found;
        }
        return null;
    }

    // Returns the category's own slug plus every descendant's slug. Used by
    // the brand listing's category filter so picking a parent category also
    // matches products that sit under any of its children.
    public static IEnumerable<string> SelfAndDescendantSlugs(this ShopCategoryVm category)
    {
        yield return category.Slug;
        foreach (var child in category.Children)
            foreach (var s in child.SelfAndDescendantSlugs())
                yield return s;
    }

    // Walks the tree to find the category with the given slug and returns
    // the path from root → target inclusive. Empty list if not found or
    // slug is null. The page uses this for breadcrumbs and the "<< parent"
    // drill-down link.
    public static IReadOnlyList<ShopCategoryVm> AncestorPath(this IEnumerable<ShopCategoryVm> roots, string? slug)
    {
        if (string.IsNullOrEmpty(slug)) return [];
        foreach (var root in roots)
        {
            var path = Walk(root, slug);
            if (path is not null) return path;
        }
        return [];

        static List<ShopCategoryVm>? Walk(ShopCategoryVm node, string slug)
        {
            if (string.Equals(node.Slug, slug, StringComparison.OrdinalIgnoreCase))
                return [node];
            foreach (var child in node.Children)
            {
                var path = Walk(child, slug);
                if (path is not null)
                {
                    path.Insert(0, node);
                    return path;
                }
            }
            return null;
        }
    }
}

public record ShopOptionVm(int Id, string Name, IReadOnlyList<string> Values);

public record ShopSkuVm(
    int Id,
    string Sku,
    IReadOnlyList<string> Opts,
    decimal Price,
    int StockQuantity,
    int LowStockThreshold,
    string Uom,
    // Optional per-SKU image. The PDP gallery pins this image as the first
    // slot when the corresponding option combination is selected, so changing
    // a variant (e.g. colour) flips the lead photo on the page.
    string? ImagePath = null)
{
    // Stock label categories mirror the legacy "in" / "low" / "order" strings
    // so the existing ShopFormatting helper keeps producing the same UI labels.
    public string StockBucket =>
        StockQuantity > LowStockThreshold ? "in"
        : StockQuantity > 0 ? "low"
        : "order";
}

public record ShopProductVm(
    int Id,
    string Slug,
    int? BrandId,
    string? BrandSlug,
    string? BrandName,
    string? CategorySlug,
    string? CategoryName,
    string Name,
    // Short is the listing teaser; Desc is the full description shown on the PDP.
    string Short,
    string Desc,
    IReadOnlyList<string> Tags,
    IReadOnlyList<ShopOptionVm> Options,
    IReadOnlyList<ShopSkuVm> Skus,
    string? PrimaryImagePath);

// Lightweight card-shape used by listings (catalog landing, brand pages,
// related strip, inspiration linked-products). Pre-computed MinPrice and
// StockBucket let the card render without enumerating per-SKU rows — that
// was the source of the N+1 PDP query problem when the previous design
// fired GetProductPdpBySlugQuery per listed product.
//
// SkuIds is kept so partner-aware price overlay can resolve actual partner
// pricing in one batched call to IPricingService.
public record ShopProductCardVm(
    int Id,
    string Slug,
    string Name,
    string Short,
    int? BrandId,
    string? BrandSlug,
    string? BrandName,
    string? CategorySlug,
    string? CategoryName,
    IReadOnlyList<string> Tags,
    decimal? MinPrice,
    string StockBucket,
    string? PrimaryImagePath,
    IReadOnlyList<int> SkuIds);

public record ShopProductInfoVm(string Key, string Value);
public record ShopProductDocVm(string Name, string Description, string Path);
public record ShopProductImageVm(string Path, string Description, int SortOrder);

// PDP carries the extra ProductDetails payload (info table, docs, gallery)
// alongside the same product/option/sku graph as the listing view.
public record ShopProductDetailVm(
    ShopProductVm Product,
    IReadOnlyList<ShopProductInfoVm> Info,
    IReadOnlyList<ShopProductDocVm> Documents,
    IReadOnlyList<ShopProductImageVm> Images);

// Pure helpers extracted from the old ShopData class — they don't touch any
// data source, they just operate on the view-model shape.
public static class ShopFormatting
{
    public static string FmtPrice(decimal p) => $"{p:N2} KM";

    // Legacy/imported SKUs often carry a supplier "No Image Available"
    // placeholder file (/no-image.jpg) as their ImagePath. That loud stock
    // graphic clashes with the site, so we treat it as "no image" and let the
    // pages fall back to their own discreet placeholder instead.
    public static bool HasRealImage(string? path) =>
        !string.IsNullOrWhiteSpace(path) &&
        !path.TrimEnd().EndsWith("no-image.jpg", StringComparison.OrdinalIgnoreCase);

    public static string? RealImageOrNull(string? path) =>
        HasRealImage(path) ? path : null;

    public static (string Label, string Color) StockLabel(string bucket) => bucket switch
    {
        "in"  => ("Na zalihi",              "var(--asg-accent)"),
        "low" => ("Niska zaliha",           "#b87320"),
        //_     => ("Po narudžbi (5–7 dana)", "var(--asg-muted)"),
        _ => ("", ""),
    };

    // Stable, opaque key for matching a selected option combination to a SKU.
    // Used by the PDP to identify which SKU the current option selection maps
    // to before adding to cart.
    public static string SkuKey(IReadOnlyList<string> opts) =>
        string.Join("|", opts.Select(v => Regex.Replace(v.ToLowerInvariant(), @"[\s./]", "")));

    public static ShopSkuVm? FindSku(ShopProductVm product, IReadOnlyDictionary<string, string> selected) =>
        product.Skus.FirstOrDefault(sku =>
            product.Options
                .Select((o, i) => (o, i))
                .All(x => selected.TryGetValue(x.o.Name, out var v) && sku.Opts[x.i] == v));

    public static bool IsAvailable(ShopProductVm product, IReadOnlyDictionary<string, string> selected, string optName, string val)
    {
        // Probe: what would the selection look like if the user picked `val` for `optName`?
        // The option is "available" if at least one SKU matches all already-locked options.
        var probe = new Dictionary<string, string>(selected) { [optName] = val };
        return product.Skus.Any(sku =>
            product.Options
                .Select((o, i) => (o, i))
                .All(x => !probe.TryGetValue(x.o.Name, out var v) || sku.Opts[x.i] == v));
    }
}
