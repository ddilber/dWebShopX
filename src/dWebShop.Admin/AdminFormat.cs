namespace dWebShop.Admin;

// Shared display formatting for the admin UI. Money is shown in KM (BAM),
// matching the storefront (dWebShop.Web ShopFormatting.FmtPrice). We do NOT use
// the "C" currency format specifier: the server runs under an invariant/en-US
// culture, so "C" renders a "$" sign, which is wrong for this market.
public static class AdminFormat
{
    // e.g. 54.95 -> "54.95 KM"
    public static string Money(decimal amount) => $"{amount:N2} KM";

    // Whole-KM variant for compact stats (e.g. dashboard revenue tiles).
    public static string Money0(decimal amount) => $"{amount:N0} KM";
}
